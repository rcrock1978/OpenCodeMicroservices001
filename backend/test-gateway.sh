#!/usr/bin/env bash
#
# Gateway End-to-End Test Script
# Tests all routes exposed through the YARP Gateway (port 5000)
# and health checks for all backend services (direct ports).
#
# Usage:
#   chmod +x test-gateway.sh
#   ./test-gateway.sh
#
# Prerequisites:
#   - All backend services + Gateway must be running
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
GATEWAY_URL="http://localhost:5000"

# Direct service ports for health checks (gateway path-transform strips /api/{svc})
# so health checks must be hit directly on each service.
HEALTH_SERVICES=(
  "identity:5001"
  "catalog:5002"
  "inventory:5003"
  "orders:5004"
  "customers:5005"
  "payments:5006"
  "notifications:5007"
)

# ── Colors ───────────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

PASS_COUNT=0
FAIL_COUNT=0

# ── Helper functions ─────────────────────────────────────────────────────────
log_info()  { echo -e "${BLUE}[INFO]${NC} $1"; }
log_pass()  { echo -e "${GREEN}[PASS]${NC} $1"; PASS_COUNT=$((PASS_COUNT+1)); }
log_fail()  { echo -e "${RED}[FAIL]${NC} $1"; FAIL_COUNT=$((FAIL_COUNT+1)); }
log_warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }

http_get() {
  local url="$1"
  local desc="${2:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" ]]; then
    log_pass "GET $desc -> HTTP $status"
  else
    log_fail "GET $desc -> HTTP $status (expected 200/201)"
  fi
}

http_get_count() {
  local url="$1"
  local desc="${2:-$url}"
  local body
  body=$(curl -s "$url" || true)
  local count
  count=$(echo "$body" | jq 'length' 2>/dev/null || echo "0")
  if [[ "$count" =~ ^[0-9]+$ && "$count" -gt 0 ]]; then
    log_pass "GET $desc -> $count items"
  else
    log_fail "GET $desc -> empty or invalid response"
  fi
}

http_post() {
  local url="$1"
  local json="$2"
  local desc="${3:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" -d "$json" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" || "$status" == "204" ]]; then
    log_pass "POST $desc -> HTTP $status"
  else
    log_fail "POST $desc -> HTTP $status (expected 200/201/204)"
  fi
}

http_post_expect() {
  local url="$1"
  local json="$2"
  local expected="$3"
  local desc="${4:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" -d "$json" "$url" || true)
  if [[ "$status" == "$expected" ]]; then
    log_pass "POST $desc -> HTTP $status"
  else
    log_fail "POST $desc -> HTTP $status (expected $expected)"
  fi
}

http_delete() {
  local url="$1"
  local desc="${2:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$url" || true)
  if [[ "$status" == "200" || "$status" == "204" || "$status" == "404" ]]; then
    log_pass "DELETE $desc -> HTTP $status"
  else
    log_fail "DELETE $desc -> HTTP $status"
  fi
}

# ── Banner ───────────────────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════════════╗"
echo "║          GATEWAY END-TO-END TEST SUITE                               ║"
echo "║          Gateway: $GATEWAY_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 1. Gateway Root ──────────────────────────────────────────────────────────
log_info "Testing Gateway root endpoint..."
http_get "$GATEWAY_URL/" "Gateway root"

# ── 2. Health Checks (direct service ports) ─────────────────────────────────
log_info ""
log_info "=== HEALTH CHECKS (direct service ports) ==="

for svc_pair in "${HEALTH_SERVICES[@]}"; do
  svc_name="${svc_pair%%:*}"
  svc_port="${svc_pair##*:}"
  for path in "/health/live" "/health/ready"; do
    http_get "http://localhost:${svc_port}${path}" "Health: $svc_name $path"
  done
done

# ── 3. Identity Service ─────────────────────────────────────────────────────
log_info ""
log_info "=== IDENTITY SERVICE ==="
http_get "$GATEWAY_URL/api/identity/users" "List users"
http_get_count "$GATEWAY_URL/api/identity/users" "Users count"
http_get "$GATEWAY_URL/api/identity/tenants" "List tenants"

# ── 4. Catalog Service ─────────────────────────────────────────────────────
log_info ""
log_info "=== CATALOG SERVICE ==="
http_get "$GATEWAY_URL/api/catalog/products" "List products"
http_get_count "$GATEWAY_URL/api/catalog/products" "Products count"
http_get "$GATEWAY_URL/api/catalog/categories" "List categories"
http_get_count "$GATEWAY_URL/api/catalog/categories" "Categories count"

# Test tenant-scoped endpoint
http_get "$GATEWAY_URL/api/catalog/products/tenant/11111111-1111-1111-1111-111111111111" "Products by tenant"

# ── 5. Inventory Service ────────────────────────────────────────────────────
log_info ""
log_info "=== INVENTORY SERVICE ==="
http_get "$GATEWAY_URL/api/inventory/inventory" "List inventory"
http_get_count "$GATEWAY_URL/api/inventory/inventory" "Inventory count"

# ── 6. Order Service ────────────────────────────────────────────────────────
log_info ""
log_info "=== ORDER SERVICE ==="
http_get "$GATEWAY_URL/api/orders/orders" "List orders"
http_get_count "$GATEWAY_URL/api/orders/orders" "Orders count"
http_get "$GATEWAY_URL/api/orders/orders/tenant/11111111-1111-1111-1111-111111111111" "Orders by tenant"

# ── 7. Customer Service ─────────────────────────────────────────────────────
log_info ""
log_info "=== CUSTOMER SERVICE ==="
http_get "$GATEWAY_URL/api/customers/customers" "List customers"
http_get_count "$GATEWAY_URL/api/customers/customers" "Customers count"
http_get "$GATEWAY_URL/api/customers/customers/tenant/11111111-1111-1111-1111-111111111111" "Customers by tenant"

# ── 8. Payment Service ────────────────────────────────────────────────────────
log_info ""
log_info "=== PAYMENT SERVICE ==="
http_get "$GATEWAY_URL/api/payments/payments/intents" "List payment intents"
http_get_count "$GATEWAY_URL/api/payments/payments/intents" "Payment intents count"
http_get "$GATEWAY_URL/api/payments/payments/methods" "List payment methods"

# ── 9. Notification Service ─────────────────────────────────────────────────
log_info ""
log_info "=== NOTIFICATION SERVICE ==="
http_get "$GATEWAY_URL/api/notifications/notifications" "List notifications"
http_get_count "$GATEWAY_URL/api/notifications/notifications" "Notifications count"
http_get "$GATEWAY_URL/api/notifications/notifications/templates" "List templates"

# ── 10. Sample Write Operations ──────────────────────────────────────────────
log_info ""
log_info "=== WRITE OPERATIONS (samples) ==="

# Create a category in Catalog
http_post "$GATEWAY_URL/api/catalog/categories" \
  '{"tenantId":"11111111-1111-1111-1111-111111111111","name":"Test Category","parentCategoryId":null}' \
  "Create category"

# Create a stock item in Inventory
http_post "$GATEWAY_URL/api/inventory/inventory" \
  '{"productVariantId":"00000000-0000-0000-0000-000000000001","sku":"TEST-SKU-001","quantityAvailable":100,"quantityReserved":0,"lowStockThreshold":10}' \
  "Create stock item"

# Create a payment method (enum values sent as integers)
http_post "$GATEWAY_URL/api/payments/payments/methods" \
  '{"tenantId":"11111111-1111-1111-1111-111111111111","customerId":"11111111-1111-1111-1111-111111111111","type":0,"lastFour":"4242","brand":"Visa","expMonth":12,"expYear":2030,"isDefault":true}' \
  "Create payment method"

# Create a notification (enum values sent as integers)
http_post "$GATEWAY_URL/api/notifications/notifications" \
  '{"tenantId":"11111111-1111-1111-1111-111111111111","recipientEmail":"test@example.com","subject":"Gateway Test","body":"This is a test notification from the gateway script.","type":0}' \
  "Create notification"

# ── 11. Auth endpoints ────────────────────────────────────────────────────────
log_info ""
log_info "=== AUTH ENDPOINTS ==="
http_post_expect "$GATEWAY_URL/api/identity/auth/login" \
  '{"email":"owner@acme.com","password":"Password123!"}' \
  "200" \
  "Login with valid credentials"

http_post_expect "$GATEWAY_URL/api/identity/auth/login" \
  '{"email":"bad@example.com","password":"wrong"}' \
  "401" \
  "Login with invalid credentials (expect 401)"

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
  echo -e "${GREEN}All tests passed!${NC}"
  exit 0
fi
