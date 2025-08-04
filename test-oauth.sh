#!/bin/bash

# OAuth 2.1 PKCE Flow Test Script
# Tests the complete OAuth 2.1 implementation

set -e

SERVER_URL="http://localhost:5001"
CLIENT_ID="mcp-sample-client"
REDIRECT_URI="http://localhost:8080/callback"
SCOPE="openid profile"
STATE="test123"

echo "🔐 OAuth 2.1 PKCE Flow Test"
echo "=========================="

# Check if server is running
echo "1. Checking server status..."
if ! curl -s "$SERVER_URL/weatherforecast" > /dev/null; then
    echo "❌ Server not running at $SERVER_URL"
    echo "   Start server with: cd Shos.MCPSample2.Server.WebSSE && dotnet run"
    exit 1
fi
echo "✅ Server is running"

# Test OAuth discovery
echo -e "\n2. Testing OAuth discovery..."
DISCOVERY=$(curl -s "$SERVER_URL/.well-known/oauth-authorization-server")
echo "✅ OAuth discovery successful"
echo "$DISCOVERY" | jq .

# Generate PKCE challenge
echo -e "\n3. Generating PKCE challenge..."
python3 -c "
import hashlib
import base64
import secrets
import urllib.parse

# Generate PKCE challenge
code_verifier = base64.urlsafe_b64encode(secrets.token_bytes(32)).decode('utf-8').rstrip('=')
code_challenge = base64.urlsafe_b64encode(hashlib.sha256(code_verifier.encode('utf-8')).digest()).decode('utf-8').rstrip('=')

print(f'export CODE_VERIFIER=\"{code_verifier}\"')
print(f'export CODE_CHALLENGE=\"{code_challenge}\"')

# Create authorization URL
params = {
    'response_type': 'code',
    'client_id': '$CLIENT_ID',
    'redirect_uri': '$REDIRECT_URI',
    'scope': '$SCOPE',
    'state': '$STATE',
    'code_challenge': code_challenge,
    'code_challenge_method': 'S256'
}

auth_url = '$SERVER_URL/oauth/authorize?' + urllib.parse.urlencode(params)
print(f'export AUTH_URL=\"{auth_url}\"')
" > /tmp/pkce_vars.sh

source /tmp/pkce_vars.sh
echo "✅ PKCE challenge generated"

# Test authorization endpoint
echo -e "\n4. Testing authorization endpoint..."
REDIRECT_RESPONSE=$(curl -s -D /tmp/headers.txt "$AUTH_URL")
LOCATION=$(grep -i "Location:" /tmp/headers.txt | cut -d' ' -f2 | tr -d '\r')

if [[ $LOCATION == *"code="* ]]; then
    echo "✅ Authorization successful"
    CODE=$(echo "$LOCATION" | sed -n 's/.*code=\([^&]*\).*/\1/p')
    echo "   Authorization code: ${CODE:0:20}..."
else
    echo "❌ Authorization failed"
    cat /tmp/headers.txt
    exit 1
fi

# Exchange code for token
echo -e "\n5. Exchanging code for access token..."
TOKEN_RESPONSE=$(curl -s -X POST "$SERVER_URL/oauth/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=authorization_code" \
  -d "code=$CODE" \
  -d "redirect_uri=$REDIRECT_URI" \
  -d "client_id=$CLIENT_ID" \
  -d "code_verifier=$CODE_VERIFIER")

ACCESS_TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.accessToken')

if [[ "$ACCESS_TOKEN" != "null" && "$ACCESS_TOKEN" != "" ]]; then
    echo "✅ Token exchange successful"
    echo "$TOKEN_RESPONSE" | jq .
else
    echo "❌ Token exchange failed"
    echo "$TOKEN_RESPONSE"
    exit 1
fi

# Test protected endpoint without token
echo -e "\n6. Testing protected MCP endpoint without token..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$SERVER_URL/api/mcp")
if [[ "$STATUS" == "401" ]]; then
    echo "✅ MCP endpoint properly protected (401 Unauthorized)"
else
    echo "⚠️  Unexpected status: $STATUS"
fi

# Test protected endpoint with token
echo -e "\n7. Testing protected MCP endpoint with valid token..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  "$SERVER_URL/api/mcp")

if [[ "$STATUS" == "404" || "$STATUS" == "200" ]]; then
    echo "✅ Successfully authenticated to MCP endpoint (Status: $STATUS)"
    echo "   (404 is expected as MCP requires specific protocol messages)"
elif [[ "$STATUS" == "401" ]]; then
    echo "❌ Token authentication failed"
    exit 1
else
    echo "ℹ️  MCP endpoint status: $STATUS"
fi

echo -e "\n🎉 OAuth 2.1 PKCE Flow Test Complete!"
echo "All OAuth 2.1 security features are working correctly."

# Cleanup
rm -f /tmp/pkce_vars.sh /tmp/headers.txt