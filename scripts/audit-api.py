"""Integration checks against an isolated local API and synthetic PostgreSQL data.

Run through run-api-audit.ps1 to create and clean up the isolated fixtures.
Never point this script at the user's application database.
"""
import datetime as dt
import json
import os
import subprocess
import urllib.error
import urllib.request
import uuid

BASE = "http://127.0.0.1:15161"
DATABASE = os.environ.get("AUDIT_DATABASE", "gocare_audit_20261114")
assert DATABASE.startswith("gocare_audit_"), "Integration tests require an isolated audit database"
PASSWORD = "Audit-only-Password123!"
PREFIX = uuid.uuid4().hex[:10]
passed, failures = [], []


def sql(statement):
    result = subprocess.run(
        ["docker", "exec", "project_worksgef-db-1", "psql", "-U", "postgres", "-d", DATABASE,
         "-t", "-A", "-v", "ON_ERROR_STOP=1", "-c", statement],
        capture_output=True, text=True, check=True)
    return result.stdout.strip()


def call(name, method, path, body=None, token=None, expected=200):
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = "Bearer " + token
    request = urllib.request.Request(BASE + path, headers=headers, method=method,
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = urllib.request.urlopen(request, timeout=30)
    except urllib.error.HTTPError as error:
        response = error
    raw = response.read().decode()
    data = json.loads(raw) if raw else None
    allowed = (expected,) if isinstance(expected, int) else expected
    if response.code in allowed:
        passed.append(name)
        print("PASS", name, flush=True)
    else:
        failures.append((name, response.code, data))
        # Responses only contain this script's synthetic fixtures.
        print("FAIL", name, response.code, json.dumps(data, ensure_ascii=True), flush=True)
    return data


def check(name, condition):
    (passed if condition else failures).append(name)
    print("PASS" if condition else "FAIL", name, flush=True)


def register(label):
    email = PREFIX + "-" + label + "@example.invalid"
    result = call(label + " registration", "POST", "/auth/register/user",
        {"email": email, "password": PASSWORD, "name": "Audit", "surname": label}, expected=201)
    account = result["accountId"]
    call(label + " blocked before verification", "POST", "/auth/login",
         {"email": email, "password": PASSWORD}, expected=403)
    verification = sql(f"SELECT token FROM email_verification_tokens WHERE account_id='{account}';")
    call(label + " verification", "POST", "/auth/verify-email", {"token": verification}, expected=204)
    session = call(label + " login", "POST", "/auth/login", {"email": email, "password": PASSWORD})
    return account, email, session


def main():
    call("API schema", "GET", "/swagger/v1/swagger.json")
    call("anonymous profile denied", "GET", "/me/profile", expected=401)
    one, email, first = register("one")
    two, _, second = register("two")
    token, other = first["accessToken"], second["accessToken"]
    call("case-insensitive login", "POST", "/auth/login", {"email": email.upper(), "password": PASSWORD})
    profile = call("profile available immediately", "GET", "/me/profile", token=token)
    check("registration retains name", profile["name"] == "Audit" and profile["surname"] == "one")
    call("profile update", "PUT", "/me/profile", {
        "name": "Audit", "surname": "One", "birthDate": "1980-01-01", "phone": "3331234567", "homeAddress": None}, token, 204)
    call("invalid profile rejected", "PUT", "/me/profile", {
        "name": "", "surname": "One", "birthDate": "2100-01-01", "phone": "x", "homeAddress": None}, token, (400, 422))
    call("municipalities lookup", "GET", "/comuni/search?q=Perugia", token=token)
    call("notification counters", "GET", "/notifications/counters", token=token)
    call("notification list", "GET", "/notifications", token=token)
    call("mark notifications read", "POST", "/notifications/read-all", token=token, expected=204)

    person_body = {"name": "Synthetic", "surname": "Assisted", "birthDate": "1940-01-01",
                   "phone": "3331234567", "homeAddress": None, "note": "Disposable audit fixture"}
    person = call("assisted creation", "POST", "/assisted", person_body, token, 201)["id"]
    call("unlinked assisted access denied", "GET", f"/assisted/{person}", token=other, expected=(403, 404))
    call("assisted update", "PUT", f"/assisted/{person}", dict(person_body, note="Updated fixture"), token, 204)
    call("last caregiver cannot be unlinked directly", "DELETE", f"/assisted/{person}/caregivers/{one}", token=token, expected=409)
    invite = call("caregiver invitation generation", "POST", f"/assisted/{person}/caregiver-invite-link", token=token)
    invite_token = invite["url"].rsplit("/", 1)[1]
    check("invitation URL uses the configured frontend origin", invite["url"].startswith(os.environ.get("AUDIT_FRONTEND_URL", "http://192.168.1.15:8081") + "/"))
    call("invitation preview", "GET", f"/caregiver-invites/{invite_token}", token=other)
    call("invitation acceptance", "POST", f"/caregiver-invites/{invite_token}/accept", token=other, expected=204)
    call("shared assisted access", "GET", f"/assisted/{person}", token=other)
    call("remove from own shared list", "DELETE", f"/assisted/{person}", token=other, expected=204)
    call("remaining caregiver retains access", "GET", f"/assisted/{person}", token=token)

    admin, admin_email, _ = register("admin")
    sql(f"UPDATE accounts SET role='Admin' WHERE id='{admin}';")
    admin_token = call("admin login", "POST", "/auth/login", {"email": admin_email, "password": PASSWORD})["accessToken"]
    address = {"street": "Via Roma", "number": "1", "postalCode": "06121", "city": "Perugia", "province": "PG"}
    association_email = PREFIX + "-association@example.invalid"
    association = call("partner association creation", "POST", "/admin/associations", {
        "email": association_email, "name": "Audit Association", "headquarter": address,
        "phones": ["3331234567"], "coveredComuni": ["054039"], "availabilityHours": "Audit only"}, admin_token, 201)["accountId"]
    invitation = sql(f"SELECT token FROM password_reset_tokens WHERE account_id='{association}' ORDER BY expires_at DESC LIMIT 1;")
    call("partner password setup", "POST", "/auth/set-password", {"token": invitation, "password": PASSWORD}, expected=204)
    assoc_token = call("partner login", "POST", "/auth/login", {"email": association_email, "password": PASSWORD})["accessToken"]
    call("association profile", "GET", "/association/profile", token=assoc_token)
    call("person denied admin endpoint", "GET", "/admin/associations", token=token, expected=403)

    departure = dt.datetime.now(dt.timezone.utc) + dt.timedelta(days=2)
    start = dict(address, latitude=43.1122, longitude=12.3888)
    end = dict(address, street="Via Settevalli", number="7", latitude=43.095, longitude=12.368)
    transport_body = {"beneficiaryId": person, "tripType": "Visit", "tripDirection": "OneWay",
        "departureDateHour": departure.isoformat(), "returnDateHour": None, "startAddress": start,
        "endAddress": end, "returnEndAddress": None, "referencePhone": "3331234567", "referenceEmail": email, "companions": []}
    for kind in ["Hospitalization", "Discharge", "Transfer"]:
        call(kind + " round-trip rejected", "POST", "/transports", dict(transport_body,
             tripType=kind, tripDirection="RoundTrip", returnDateHour=(departure + dt.timedelta(hours=2)).isoformat()), token, 422)
    trip = call("transport creation and real routing", "POST", "/transports", transport_body, token, 201)["id"]
    call("transport list", "GET", "/transports", token=token)
    call("transport detail", "GET", f"/transports/{trip}", token=token)
    call("unlinked transport denied", "GET", f"/transports/{trip}", token=other, expected=(403, 404))
    call("last caregiver removal blocked by open trip", "DELETE", f"/assisted/{person}", token=token, expected=409)
    call("pending cancellation without Stripe", "POST", f"/transports/{trip}/cancel", token=token, expected=204)
    round_trip = call("round trip without scheduled return time", "POST", "/transports",
                      dict(transport_body, tripDirection="RoundTrip", returnDateHour=None), token, 201)["id"]
    round_detail = call("unscheduled round-trip detail", "GET", f"/transports/{round_trip}", token=token)
    check("return remains unscheduled", round_detail["returnDateHour"] is None)
    call("unscheduled round-trip cancellation", "POST", f"/transports/{round_trip}/cancel", token=token, expected=204)

    trip = call("second transport creation", "POST", "/transports", transport_body, token, 201)["id"]
    call("candidate request list", "GET", "/association/requests", token=assoc_token)
    call("association accepts transport", "POST", f"/association/requests/{trip}/accept", token=assoc_token, expected=204)
    call("accepted transport detail", "GET", f"/association/transports/{trip}", token=assoc_token)
    call("accepted requester contacts", "GET", f"/association/requests/{trip}/contacts", token=assoc_token)
    call("missing payment configuration handled", "POST", f"/transports/{trip}/payment-session", token=token, expected=503)
    operator = call("operator link", "POST", f"/association/transports/{trip}/operator-link", token=assoc_token)
    operator_token = operator["url"].rsplit("/", 1)[1]
    call("operator status", "GET", f"/operator/{operator_token}/status")
    for label in ["start", "arrive", "complete"]:
        call("operator " + label, "POST", f"/operator/{operator_token}/status", {"action": "Advance", "operatorLabel": "Audit"})
    call("completed trip cannot be cancelled", "POST", f"/transports/{trip}/cancel", token=token, expected=409)
    call("association history", "GET", "/association/history", token=assoc_token)
    call("trip timeline", "GET", f"/transports/{trip}/status-timeline", token=token)
    cancelled_trip = call("cancellation fixture creation", "POST", "/transports", transport_body, token, 201)["id"]
    call("cancellation fixture acceptance", "POST", f"/association/requests/{cancelled_trip}/accept", token=assoc_token, expected=204)
    cancelled_operator = call("operator link before cancellation", "POST", f"/association/transports/{cancelled_trip}/operator-link", token=assoc_token)
    cancelled_operator_token = cancelled_operator["url"].rsplit("/", 1)[1]
    call("confirmed unpaid cancellation without Stripe", "POST", f"/transports/{cancelled_trip}/cancel", token=token, expected=204)
    call("operator cannot start cancelled trip", "POST", f"/operator/{cancelled_operator_token}/status",
         {"action": "Advance", "operatorLabel": "Audit"}, expected=(403, 409))
    call("archive sole-caregiver assisted after completion", "DELETE", f"/assisted/{person}", token=token, expected=204)
    call("archived assisted hidden", "GET", f"/assisted/{person}", token=token, expected=404)
    call("archived assisted invitation cannot be accepted", "POST", f"/caregiver-invites/{invite_token}/accept", token=other, expected=(403, 404, 409))

    rotated = call("refresh-token rotation", "POST", "/auth/refresh", {"refreshToken": first["refreshToken"]})
    call("used refresh-token rejected", "POST", "/auth/refresh", {"refreshToken": first["refreshToken"]}, expected=403)
    call("logout", "POST", "/auth/logout", {"refreshToken": rotated["refreshToken"]}, rotated["accessToken"], 204)
    call("logged-out refresh rejected", "POST", "/auth/refresh", {"refreshToken": rotated["refreshToken"]}, expected=403)
    print(f"\n{len(passed)} passed; {len(failures)} failed.", flush=True)
    raise SystemExit(bool(failures))


if __name__ == '__main__':
    main()
