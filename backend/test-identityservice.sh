#!/usr/bin/env bash
#
# IdentityService Endpoint Test Script
# Tests all API endpoints for the Identity Service directly
#
# Usage:
#   chmod +x test-identityservice.sh
#   ./test-identityservice.sh
#
# Prerequisites:
#   - IdentityService must be running (default port 5001)
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
BASE_URL="http://localhost:5001"
DEMO_TENANT_ID="11111111-1111-1111-1111-111111111111"
DEMO_USER_ID="aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
DEMO_EMAIL="owner@acme.com"
DEMO_PASSWORD="Password123!"

# ── Colors ───────────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

PASS_COUNT=0
FAIL_COUNT=0

# ── Helper functions ─────────────────────────────────────────────────────────
log_info()  { echo -e "${BLUE}[INFO]${NC} $1"; }
log_pass()  { echo -e "${GREEN}[PASS]${NC} $1"; PASS_COUNT=$((PASS_COUNT+1)); }
log_fail()  { echo -e "${RED}[FAIL]${NC} $1"; FAIL_COUNT=$((FAIL_COUNT+1)); }
log_warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_step()  { echo -e "${CYAN}[STEP]${NC} $1"; }

http_get() {
  local url="$1"
  local desc="${2:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" ]]; then
    log_pass "GET $desc -> HTTP $status"
    return 0
  else
    log_fail "GET $desc -> HTTP $status (expected 200/201)"
    return 1
  fi
}

http_get_body() {
  local url="$1"
  curl -s "$url" || true
}

http_post() {
  local url="$1"
  local json="$2"
  local desc="${3:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" -d "$json" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" ]]; then
    log_pass "POST $desc -> HTTP $status"
    return 0
  else
    log_fail "POST $desc -> HTTP $status (expected 200/201)"
    return 1
  fi
}

http_post_body() {
  local url="$1"
  local json="$2"
  curl -s -X POST -H "Content-Type: application/json" -d "$json" "$url" || true
}

http_put() {
  local url="$1"
  local json="$2"
  local desc="${3:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X PUT -H "Content-Type: application/json" -d "$json" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" || "$status" == "204" ]]; then
    log_pass "PUT $desc -> HTTP $status"
    return 0
  else
    log_fail "PUT $desc -> HTTP $status (expected 200/201/204)"
    return 1
  fi
}

http_delete() {
  local url="$1"
  local desc="${2:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$url" || true)
  if [[ "$status" == "200" || "$status" == "204" ]]; then
    log_pass "DELETE $desc -> HTTP $status"
    return 0
  else
    log_fail "DELETE $desc -> HTTP $status (expected 200/204)"
    return 1
  fi
}

http_expect() {
  local method="$1"
  local url="$2"
  local expected="$3"
  local desc="${4:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X "$method" "$url" || true)
  if [[ "$status" == "$expected" ]]; then
    log_pass "$method $desc -> HTTP $status"
    return 0
  else
    log_fail "$method $desc -> HTTP $status (expected $expected)"
    return 1
  fi
}

# ── Banner ───────────────────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════════════╗"
echo "║          IDENTITY SERVICE ENDPOINT TEST SUITE                        ║"
echo "║          Target: $BASE_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 0. Service Health ────────────────────────────────────────────────────────
log_step "Service Health"
http_get "$BASE_URL/" "Root endpoint"
http_get "$BASE_URL/health/live" "Health: live"
http_get "$BASE_URL/health/ready" "Health: ready"

# ── 1. Auth Endpoints ────────────────────────────────────────────────────────
log_step "Auth Endpoints"

# Login with valid credentials
log_info "Testing login with demo credentials..."
LOGIN_RESPONSE=$(http_post_body "$BASE_URL/api/auth/login" \
  "{\"Email\":\"$DEMO_EMAIL\",\"Password\":\"$DEMO_PASSWORD\"}")
LOGIN_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" \
  -d "{\"Email\":\"$DEMO_EMAIL\",\"Password\":\"$DEMO_PASSWORD\"}" "$BASE_URL/api/auth/login" || true)

if [[ "$LOGIN_STATUS" == "200" ]]; then
  TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token // empty' 2>/dev/null || true)
  if [[ -n "$TOKEN" && "$TOKEN" != "null" ]]; then
    log_pass "Login with valid credentials -> JWT received"
  else
    log_fail "Login with valid credentials -> No JWT token in response"
  fi
else
  log_fail "Login with valid credentials -> HTTP $LOGIN_STATUS (expected 200)"
fi

# Login with invalid credentials
http_expect "POST" "$BASE_URL/api/auth/login" "401" \
  "Login with invalid credentials (expect 401)" \
  '{"Email":"bad@example.com","Password":"wrongpassword"}'

# Login with inactive user
http_expect "POST" "$BASE_URL/api/auth/login" "401" \
  "Login with inactive user (expect 401)" \
  '{"Email":"inactive@techstart.com","Password":"Password123!"}'

# Register a new user
log_info "Testing user registration..."
REGISTER_RESPONSE=$(http_post_body "$BASE_URL/api/auth/register" \
  "{\"Email\":\"newuser.$(date +%s)@example.com\",\"Password\":\"Password123!\",\"DisplayName\":\"New User\",\"TenantId\":\"$DEMO_TENANT_ID\"}")
REGISTER_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" \
  -d "{\"Email\":\"newuser.$(date +%s)@example.com\",\"Password\":\"Password123!\",\"DisplayName\":\"New User\",\"TenantId\":\"$DEMO_TENANT_ID\"}" "$BASE_URL/api/auth/register" || true)

if [[ "$REGISTER_STATUS" == "201" ]]; then
  NEW_USER_ID=$(echo "$REGISTER_RESPONSE" | jq -r '.userId // empty' 2>/dev/null || true)
  log_pass "Register new user -> ID: $NEW_USER_ID"
else
  log_fail "Register new user -> HTTP $REGISTER_STATUS (expected 201)"
fi

# ── 2. Tenant Endpoints ────────────────────────────────────────────────────
log_step "Tenant Endpoints"

# List all tenants
http_get "$BASE_URL/api/tenants" "List all tenants"
TENANT_LIST=$(http_get_body "$BASE_URL/api/tenants")
TENANT_COUNT=$(echo "$TENANT_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $TENANT_COUNT tenants"

# Get tenant by ID
http_get "$BASE_URL/api/tenants/$DEMO_TENANT_ID" "Get tenant by ID"

# Get tenant by subdomain
http_get "$BASE_URL/api/tenants/by-subdomain/acme" "Get tenant by subdomain (acme)"

# Test 404 for non-existent tenant
http_expect "GET" "$BASE_URL/api/tenants/00000000-0000-0000-0000-000000000000" "404" "Get non-existent tenant (expect 404)"

# Create a new tenant
log_info "Creating a test tenant..."
CREATE_TENANT_RESPONSE=$(http_post_body "$BASE_URL/api/tenants" \
  "{\"Name\":\"Test Corp $(date +%s)\",\"Subdomain\":\"testcorp-$(date +%s)\",\"SubscriptionPlanId\":\"plan_starter\"}")
NEW_TENANT_ID=$(echo "$CREATE_TENANT_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_TENANT_ID" && "$NEW_TENANT_ID" != "null" ]]; then
  log_pass "Create tenant -> ID: $NEW_TENANT_ID"

  # Verify tenant by ID
  http_get "$BASE_URL/api/tenants/$NEW_TENANT_ID" "Get created tenant"
else
  log_fail "Create tenant -> No ID returned"
fi

# Update tenant
if [[ -n "$NEW_TENANT_ID" && "$NEW_TENANT_ID" != "null" ]]; then
  http_put "$BASE_URL/api/tenants/$NEW_TENANT_ID" \
    "{\"Name\":\"Updated Corp\",\"IsActive\":true}" \
    "Update tenant"
fi

# ── 3. User Endpoints ────────────────────────────────────────────────────────
log_step "User Endpoints"

# List all users
http_get "$BASE_URL/api/users" "List all users"
USER_LIST=$(http_get_body "$BASE_URL/api/users")
USER_COUNT=$(echo "$USER_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $USER_COUNT users"

# Get user by ID
http_get "$BASE_URL/api/users/$DEMO_USER_ID" "Get user by ID"

# Test 404 for non-existent user
http_expect "GET" "$BASE_URL/api/users/00000000-0000-0000-0000-000000000000" "404" "Get non-existent user (expect 404)"

# Create a user (admin role)
log_info "Creating a test user..."
CREATE_USER_RESPONSE=$(http_post_body "$BASE_URL/api/users" \
  "{\"Email\":\"admin.$(date +%s)@example.com\",\"Password\":\"Password123!\",\"DisplayName\":\"Test Admin\",\"TenantId\":\"$DEMO_TENANT_ID\",\"Role\":\"Admin\"}")
NEW_USER_ID_2=$(echo "$CREATE_USER_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_USER_ID_2" && "$NEW_USER_ID_2" != "null" ]]; then
  log_pass "Create user -> ID: $NEW_USER_ID_2"

  # Verify user exists
  http_get "$BASE_URL/api/users/$NEW_USER_ID_2" "Get created user"

  # Delete the user
  http_delete "$BASE_URL/api/users/$NEW_USER_ID_2" "Delete user"
else
  log_fail "Create user -> No ID returned"
fi

# ── Summary ───────────────────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════════════╗"
echo "║                         TEST SUMMARY                                 ║"
echo "╠══════════════════════════════════════════════════════════════════════╣"
printf  "║  ${GREEN}PASSED:${NC} %-4d                                                  ║\n" "$PASS_COUNT"
printf  "║  ${RED}FAILED:${NC} %-4d                                                  ║\n" "$FAIL_COUNT"
printf  "║  TOTAL:  %-4d                                                      ║\n" "$((PASS_COUNT + FAIL_COUNT))"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

if [[ "$FAIL_COUNT" -gt 0 ]]; then
  echo -e "${RED}Some tests failed. Please review the output above.${NC}"
  exit 1
else
  echo -e "${GREEN}All IdentityService tests passed!${NC}"
  exit 0
fi
