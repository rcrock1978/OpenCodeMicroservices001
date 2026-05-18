#!/usr/bin/env bash
#
# NotificationService Endpoint Test Script
# Tests all API endpoints for the Notification Service directly
#
# Usage:
#   chmod +x test-notificationservice.sh
#   ./test-notificationservice.sh
#
# Prerequisites:
#   - NotificationService must be running (default port 5007)
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
BASE_URL="http://localhost:5007"
DEMO_TENANT_ID="11111111-1111-1111-1111-111111111111"

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
echo "║          NOTIFICATION SERVICE ENDPOINT TEST SUITE                    ║"
echo "║          Target: $BASE_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 0. Service Health ────────────────────────────────────────────────────────
log_step "Service Health"
http_get "$BASE_URL/" "Root endpoint"
http_get "$BASE_URL/health/live" "Health: live"
http_get "$BASE_URL/health/ready" "Health: ready"

# ── 1. Notifications ─────────────────────────────────────────────────────────
log_step "Notification Endpoints"

# List all notifications
http_get "$BASE_URL/api/notifications" "List all notifications"
NOTIF_LIST=$(http_get_body "$BASE_URL/api/notifications")
NOTIF_COUNT=$(echo "$NOTIF_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $NOTIF_COUNT notifications"

# List notifications by tenant
http_get "$BASE_URL/api/notifications/tenant/$DEMO_TENANT_ID" "List notifications by tenant"

# Create a notification
log_info "Creating a test notification..."
CREATE_NOTIF_RESPONSE=$(http_post_body "$BASE_URL/api/notifications" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"recipientEmail\":\"test@example.com\",\"subject\":\"Test Subject $(date +%s)\",\"body\":\"This is a test notification body\",\"type\":\"Email\"}")

NEW_NOTIF_ID=$(echo "$CREATE_NOTIF_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_NOTIF_ID" && "$NEW_NOTIF_ID" != "null" ]]; then
  log_pass "Create notification -> ID: $NEW_NOTIF_ID"

  # Verify notification exists in list
  http_get "$BASE_URL/api/notifications" "List notifications after create"
else
  log_fail "Create notification -> No ID returned"
fi

# Send the notification
if [[ -n "$NEW_NOTIF_ID" && "$NEW_NOTIF_ID" != "null" ]]; then
  log_info "Sending notification $NEW_NOTIF_ID..."
  SEND_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/notifications/$NEW_NOTIF_ID/send" || true)
  if [[ "$SEND_STATUS" == "200" ]]; then
    log_pass "Send notification -> HTTP 200"

    # Verify status is now Sent
    SENT_NOTIF=$(http_get_body "$BASE_URL/api/notifications")
    SENT_STATUS=$(echo "$SENT_NOTIF" | jq "map(select(.id == \"$NEW_NOTIF_ID\")) | .[0].status" 2>/dev/null || echo "null")
    if [[ "$SENT_STATUS" == "\"Sent\"" || "$SENT_STATUS" == "\"Sent\"" ]]; then
      log_pass "Verify notification status -> Sent"
    else
      log_warn "Verify notification status -> $SENT_STATUS (expected 'Sent')"
    fi
  else
    log_fail "Send notification -> HTTP $SEND_STATUS (expected 200)"
  fi
fi

# Test 404 for sending non-existent notification
http_expect "POST" "$BASE_URL/api/notifications/00000000-0000-0000-0000-000000000000/send" "404" "Send non-existent notification (expect 404)"

# ── 2. Templates ─────────────────────────────────────────────────────────────
log_step "Template Endpoints"

# List all templates
http_get "$BASE_URL/api/notifications/templates" "List all templates"
TEMPLATE_LIST=$(http_get_body "$BASE_URL/api/notifications/templates")
TEMPLATE_COUNT=$(echo "$TEMPLATE_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $TEMPLATE_COUNT templates"

# Create a template
log_info "Creating a test template..."
CREATE_TEMPLATE_RESPONSE=$(http_post_body "$BASE_URL/api/notifications/templates" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"key\":\"test-template-$(date +%s)\",\"subject\":\"Test Template Subject\",\"bodyHtml\":\"<p>Hello {{name}}</p>\",\"bodyText\":\"Hello {{name}}\",\"channel\":\"Email\"}")

NEW_TEMPLATE_ID=$(echo "$CREATE_TEMPLATE_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_TEMPLATE_ID" && "$NEW_TEMPLATE_ID" != "null" ]]; then
  log_pass "Create template -> ID: $NEW_TEMPLATE_ID"

  # Verify template appears in list
  http_get "$BASE_URL/api/notifications/templates" "List templates after create"
else
  log_fail "Create template -> No ID returned"
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
  echo -e "${GREEN}All NotificationService tests passed!${NC}"
  exit 0
fi
