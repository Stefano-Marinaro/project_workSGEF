# GoCare — schema logico (DBML per dbdiagram.io)

Incolla il blocco sotto su https://dbdiagram.io (nuovo progetto → cancella il placeholder →
incolla). Allineato allo schema reale generato dalle migrazioni EF applicate
(`Initial` su `gocare_business`, `InitialAuth` su `gocare_auth`), non a una bozza.

Convenzioni riportate qui:
- Tipi enum come `text` nel DB reale (conversione EF enum→string); qui uso il tipo
  `enum` di DBML solo per leggibilità del diagramma — non cambia lo schema.
- `notifications.subject_id`, `device_tokens.subject_id`, `transport_requests.deleted_by`
  e `accounts.id` ↔ `persons.id`/`associations.id` sono **riferimenti polimorfici o
  cross-database senza FK reale**: le `Ref` sotto sono solo documentazione visiva (due
  linee verso i due target possibili), non vincoli effettivi.
- `xmin` (concurrency token su `transport_requests`) omesso: colonna di sistema
  Postgres, non struttura di dominio.

```dbml
Enum e_account_role {
  Person
  Association
}

Enum e_account_status {
  Unverified
  Active
  Suspended
  Deleted
}

Enum e_accreditation_status {
  Pending
  Accredited
  Rejected
}

Enum e_trip_type {
  Visit
  Hospitalization
  Discharge
  Transfer
}

Enum e_trip_direction {
  OnlyGo
  OnlyReturn
  RoundTrip
}

Enum e_trip_request_status {
  Pending
  Confirmed
  InProgress
  Completed
  NotCovered
  Cancelled
}

Enum e_trip_transition_status {
  Pending
  InCharge
  Arriving
  OnSite
  Returning
  Completed
}

Enum e_rejection_kind {
  Declined
  CancelledAfterAcceptance
}

Enum e_modification_field {
  Schedule
  EndAddress
  ReturnEndAddress
}

Enum e_modification_request_status {
  PendingApproval
  Approved
  Rejected
  Retracted
}

Enum e_group_role {
  Caregiver
  Assisted
}

Enum e_group_admin_role {
  Admin
  Member
}

Enum e_invitation_group_status {
  Pending
  Accepted
  Refused
  Revoked
}

Enum e_notification_subject {
  Association
  Person
}

Enum e_contact_data_kind {
  Requester
  Beneficiary
}

Enum e_device_platform {
  Ios
  Android
  Web
}

// ==================== gocare_auth ====================

Table accounts {
  id uuid [pk, note: 'PK condivisa con persons.id o associations.id (altro DB, nessuna FK reale) — vedi Ref sotto']
  email varchar(255) [unique, not null]
  password_hash text [not null]
  role e_account_role [not null]
  status e_account_status [not null, default: 'Unverified']
  created_at timestamptz [not null]
  email_verified_at timestamptz
  suspended_at timestamptz
  deleted_at timestamptz
}

Table email_verification_tokens {
  id uuid [pk]
  account_id uuid [not null]
  token varchar(255) [unique, not null]
  expires_at timestamptz [not null]
  consumed_at timestamptz
}

Table password_reset_tokens {
  id uuid [pk]
  account_id uuid [not null]
  token varchar(255) [unique, not null]
  expires_at timestamptz [not null]
  consumed_at timestamptz
}

Table refresh_tokens {
  id uuid [pk]
  account_id uuid [not null]
  token varchar(255) [unique, not null]
  expires_at timestamptz [not null]
  created_at timestamptz [not null]
  revoked_at timestamptz
}

Table failed_login_attempts {
  id uuid [pk]
  email varchar(255) [not null, note: 'per e-mail, NON FK: tentativi anche su indirizzi inesistenti']
  attempted_at timestamptz [not null]
  ip_address varchar(45)

  Indexes {
    (email, attempted_at)
  }
}

Ref: email_verification_tokens.account_id > accounts.id [delete: cascade]
Ref: password_reset_tokens.account_id > accounts.id [delete: cascade]
Ref: refresh_tokens.account_id > accounts.id [delete: cascade]

// ==================== gocare_business ====================

Table persons {
  id uuid [pk, note: 'PK condivisa con accounts.id (altro DB, nessuna FK reale)']
  name varchar(100) [not null]
  surname varchar(100) [not null]
  birth_date date [not null]
  email varchar(255) [not null]
  phone varchar(32) [not null]
  home_address_street varchar(200)
  home_address_number varchar(20)
  home_address_postal_code varchar(10)
  home_address_city varchar(120)
  home_address_province varchar(120)
  deleted_at timestamptz
  anonymized_at timestamptz
}

Table associations {
  id uuid [pk, note: 'PK condivisa con accounts.id (altro DB, nessuna FK reale)']
  name varchar(200) [not null]
  headquarter_street varchar(200) [not null]
  headquarter_number varchar(20) [not null]
  headquarter_postal_code varchar(10) [not null]
  headquarter_city varchar(120) [not null]
  headquarter_province varchar(120) [not null]
  phones "text[]" [not null]
  email varchar(255) [not null]
  availability_hours varchar(200)
  status e_accreditation_status [not null, default: 'Pending']
  covered_provinces "text[]" [not null, note: 'PA-05: instradamento richieste per provincia']
  deleted_at timestamptz
}

Table care_groups {
  id uuid [pk]
  name varchar(200) [not null]
  description varchar(255)
  created_by uuid [not null]
  deleted_at timestamptz

  Indexes {
    (created_by, name) [unique]
  }
}

Table care_group_memberships {
  care_group_id uuid [not null]
  person_id uuid [not null]
  admin_role e_group_admin_role [not null]
  role e_group_role [not null]
  status e_invitation_group_status [not null]
  invitation_email varchar(255)
  invitation_token varchar(255) [unique]
  created_at timestamptz [not null]
  responded_at timestamptz
  deleted_at timestamptz

  Indexes {
    (care_group_id, person_id) [pk]
  }
}

Table saved_destinations {
  id uuid [pk]
  person_id uuid [not null]
  place_name varchar(200) [not null]
  saved_address_street varchar(200) [not null]
  saved_address_number varchar(20) [not null]
  saved_address_postal_code varchar(10) [not null]
  saved_address_city varchar(120) [not null]
  saved_address_province varchar(120) [not null]
  note varchar(300)
}

Table transport_requests {
  id uuid [pk]
  requested_by_id uuid [not null]
  beneficiary_id uuid [not null]
  care_group_id uuid
  trip_type e_trip_type [not null]
  trip_direction e_trip_direction [not null]
  departure_date_hour timestamptz [not null]
  return_date_hour timestamptz
  start_address_street varchar(200) [not null]
  start_address_number varchar(20) [not null]
  start_address_postal_code varchar(10) [not null]
  start_address_city varchar(120) [not null]
  start_address_province varchar(120) [not null]
  end_address_street varchar(200) [not null]
  end_address_number varchar(20) [not null]
  end_address_postal_code varchar(10) [not null]
  end_address_city varchar(120) [not null]
  end_address_province varchar(120) [not null]
  return_end_address_street varchar(200)
  return_end_address_number varchar(20)
  return_end_address_postal_code varchar(10)
  return_end_address_city varchar(120)
  return_end_address_province varchar(120)
  reference_phone varchar(32) [not null]
  reference_email varchar(255) [not null]
  created_at timestamptz [not null]
  request_status e_trip_request_status [not null, default: 'Pending']
  assigned_association_id uuid
  deleted_by uuid [note: 'polimorfico: Person o Association, nessuna FK']
  accepted_at timestamptz
  not_covered_at timestamptz
  deleted_at timestamptz

  Indexes {
    start_address_province
    request_status
  }
}

Table transport_request_candidates {
  transport_request_id uuid [not null]
  association_id uuid [not null]

  Indexes {
    (transport_request_id, association_id) [pk]
  }
}

Table transport_request_rejections {
  id uuid [pk]
  transport_request_id uuid [not null]
  association_id uuid [not null]
  kind e_rejection_kind [not null]
  reason varchar(300)
  rejected_at timestamptz [not null]
}

Table companions {
  id uuid [pk]
  transport_request_id uuid [not null]
  name varchar(200) [not null]
  surname varchar(200) [not null]
  relationship varchar(200) [not null]
  phone varchar(32) [not null]
}

Table transport_modification_requests {
  id uuid [pk]
  transport_request_id uuid [not null]
  field e_modification_field [not null]
  previous_value text [not null]
  proposed_value text [not null]
  status e_modification_request_status [not null, default: 'PendingApproval']
  outcome_message varchar(300)
  created_at timestamptz [not null]
  resolved_at timestamptz
}

Table trip_status_transitions {
  id uuid [pk]
  transport_request_id uuid [not null]
  status e_trip_transition_status [not null]
  made_by_association_id uuid [not null]
  operator_label varchar(200) [not null]
  occurred_at timestamptz [not null]

  Indexes {
    (transport_request_id, occurred_at)
  }
}

Table notifications {
  id uuid [pk]
  subject_type e_notification_subject [not null, note: 'polimorfico + subject_id: nessuna FK']
  subject_id uuid [not null]
  type varchar [not null]
  title varchar(200) [not null]
  body varchar(1000) [not null]
  channels integer [not null, note: '[Flags] Push=1, Email=2 — salvato come int, non text']
  related_entity_id uuid [note: 'polimorfico, nessuna FK']
  created_at timestamptz [not null]
  read_at timestamptz

  Indexes {
    (subject_type, subject_id)
  }
}

Table contact_access_logs {
  id uuid [pk]
  transport_request_id uuid [not null]
  association_id uuid [not null]
  data_kind e_contact_data_kind [not null]
  accessed_at timestamptz [not null]
}

Table device_tokens {
  id uuid [pk]
  subject_type e_notification_subject [not null, note: 'polimorfico + subject_id: nessuna FK']
  subject_id uuid [not null]
  push_token text [not null, note: 'nessun limite di lunghezza: lo decide FCM/APNs']
  platform e_device_platform [not null]
  created_at timestamptz [not null]
  deactivated_at timestamptz

  Indexes {
    (subject_type, subject_id)
  }
}

// ---- relazioni reali (FK effettive in BusinessDbContext, tutte OnDelete Restrict) ----

Ref: care_groups.created_by > persons.id
Ref: care_group_memberships.care_group_id > care_groups.id
Ref: care_group_memberships.person_id > persons.id
Ref: saved_destinations.person_id > persons.id
Ref: transport_requests.requested_by_id > persons.id
Ref: transport_requests.beneficiary_id > persons.id
Ref: transport_requests.care_group_id > care_groups.id
Ref: transport_requests.assigned_association_id > associations.id
Ref: transport_request_candidates.transport_request_id > transport_requests.id
Ref: transport_request_candidates.association_id > associations.id
Ref: transport_request_rejections.transport_request_id > transport_requests.id
Ref: transport_request_rejections.association_id > associations.id
Ref: companions.transport_request_id > transport_requests.id
Ref: transport_modification_requests.transport_request_id > transport_requests.id
Ref: trip_status_transitions.transport_request_id > transport_requests.id
Ref: trip_status_transitions.made_by_association_id > associations.id
Ref: contact_access_logs.transport_request_id > transport_requests.id
Ref: contact_access_logs.association_id > associations.id

// ---- relazioni solo documentarie (nessuna FK reale nel DB) ----

Ref: accounts.id - persons.id
Ref: accounts.id - associations.id
Ref: notifications.subject_id > persons.id
Ref: notifications.subject_id > associations.id
Ref: device_tokens.subject_id > persons.id
Ref: device_tokens.subject_id > associations.id

TableGroup gocare_auth {
  accounts
  email_verification_tokens
  password_reset_tokens
  refresh_tokens
  failed_login_attempts
}

TableGroup gocare_business {
  persons
  associations
  care_groups
  care_group_memberships
  saved_destinations
  transport_requests
  transport_request_candidates
  transport_request_rejections
  companions
  transport_modification_requests
  trip_status_transitions
  notifications
  contact_access_logs
  device_tokens
}
```
