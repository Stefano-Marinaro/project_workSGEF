# App health check — 14 November 2026

Performed without a browser, browser automation, or visual UI inspection.
Existing application accounts and trips were not used as test fixtures. API checks ran against
a separate localhost-only API container and disposable PostgreSQL database, both removed afterward.

## Results

| Check | Result |
| --- | --- |
| Backend unit/regression tests | 101 passed |
| Isolated HTTP integration checks | 75 passed |
| Frontend session regression tests | 3 passed |
| Frontend source checks | 72 files parsed and checked for undefined identifiers; 68 literal navigation targets, 51 literal icon names and 59 API calls checked; no mismatches |
| Expo dependency compatibility | Passed after compatible SDK 57 patch updates |
| Docker builds | Backend compiled and frontend clean install/export passed; both containers updated |

The API checks cover registration, blocked login before verification, email verification, login,
case-insensitive login, profile read/update/validation, municipality search, notifications,
assisted-person creation/update/access control, caregiver invitations and acceptance, removal from
a shared list, sole-caregiver archival, administrator authorization, partner association creation
and password setup, transport validation/creation/list/detail, real GraphHopper routing,
cancellation of pending and confirmed unpaid trips without Stripe, association acceptance,
requester contacts, operator links and progression through completion, history/timeline,
invalidating operator actions after cancellation, refresh-token rotation/reuse, and logout.

## Fixed

- **New caregiver invitations immediately appeared expired.** Getter-only `ExpiresAt` and `CreatedAt`
  were not included in the EF model or database. Added mappings, a generated migration and a reload
  regression test. Existing links have no recoverable original expiry; the migration keeps those
  expired. Generate new invitations after updating.
- **Expired access tokens could trigger simultaneous refresh rotations and unexpected logout.**
  Added one shared refresh operation, reuse of a token already refreshed by another request, and
  preservation of the session after transient refresh-server/network errors. Added three regression tests.
- **Login, password-reset requests and verification-email requests depended on email capitalization.**
  Normalized lookup and registration/change-email duplicate comparisons.
- **An old invitation could relink an archived assisted person.** Acceptance now checks that the
  person is active and the inviter is still linked; preview also checks the inviter's link.
- **Operator links returned an internal error after cancellation.** They now return a controlled
  denial. Links from an earlier association acceptance cannot control a later reassignment.
- **Email/invitation/operator links used localhost on phones.** Added `FRONTEND_PUBLIC_URL` for all
  generated frontend links; configured the current local installation with its existing LAN origin.
- **Missing frontend destinations for generated links.** Added email-change confirmation and the
  two Stripe return pages. A return page never marks a payment as successful; only the webhook does.
- **Copying invitation links could fail without feedback.** Added an error message and manual-copy/QR fallback guidance.
- Corrected status-bar props/imports and aligned Expo, Expo Constants, Expo Linking and Expo Router
  to SDK 57 compatible patch versions. Applied compatible npm security patches without major upgrades.
- Regenerated the npm lockfile using Docker's Node 22/npm 10 environment after the Windows npm 12
  lockfile omitted a required peer entry; a clean Docker `npm ci` and Expo web export then passed.

## Remaining gaps and limits

1. **Mobility and assistance selections are not persisted.** The wizard collects them, but
   `NewTransportRequest` has no corresponding fields; associations do not receive these selections.
2. **Additional donation is not persisted or charged.** The frontend includes it in its displayed
   total but the backend checkout calculates only distance tariff/minimum. This needs an agreed
   end-to-end data/payment implementation, not just a visual change.
3. **Payments are incomplete in this installation.** Stripe credentials are blank; the missing-key
   failure returns a controlled 503. The caregiver detail page has no checkout action yet. No real
   charge, refund or Stripe webhook delivery was tested. Some wizard payment copy still describes
   payment at service time, while the implemented backend supports Stripe checkout after acceptance.
4. **Email delivery is not configured.** Account flows were verified using tokens from the isolated
   test database, with SMTP disabled. Real email delivery and push delivery were not tested.
5. **Support contacts, privacy URL and terms URL remain unconfigured.** Their pages explicitly say
   those resources are unavailable. The change-password screen is still a placeholder.
6. **Dependency advisories remain.** After compatible frontend fixes, npm still reports six
   advisories (three moderate and three high), involving the Expo dependency tree and router's
   `query-string`/`decode-uri-component` dependency. Automated recommendations require an SDK major
   change; none was forced. Backend builds also report advisories affecting MailKit 4.9.0,
   MimeKit 4.9.0 and System.Security.Cryptography.Pkcs 8.0.1. These require a separate dependency update.
7. **No visual/device verification.** Layout, touch behavior, keyboard interactions, browser storage
   restrictions, actual iPhone/Android execution and device-specific clipboard behavior remain
   outside this browser-free check. A successful build/static check does not prove those behaviors.
8. The phone URL depends on the PC retaining its current LAN address and being reachable through
   the local network/firewall. Production deployment/authentication settings were not assessed.

## Repeatable checks

Run from the repository root after installing the locked frontend dependencies:

```powershell
dotnet test BackEnd/tests/GoCare.Tests/GoCare.Tests.csproj
node --test scripts/http-client.test.cjs
node scripts/audit-frontend.cjs
```

The frontend contract check reads the running backend's Swagger schema; it requires Development mode.
For isolated integration checks, first build `project_worksgef-backend` and keep the project database
container running, then run:

```powershell
./scripts/run-api-audit.ps1
```

The integration runner requires Python, the existing Docker Compose network/container names, a free
localhost port 15161, and a valid GraphHopper key in `.env`. It uses synthetic `.invalid` email
addresses, disables SMTP and Stripe, performs real routing requests with public sample coordinates,
and removes its own API container and disposable database in `finally`.
