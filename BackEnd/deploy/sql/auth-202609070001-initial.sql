BEGIN;

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

CREATE TABLE IF NOT EXISTS accounts (
    id uuid NOT NULL PRIMARY KEY,
    email character varying(320) NOT NULL,
    password_hash text NOT NULL,
    role character varying(32) NOT NULL,
    status character varying(32) NOT NULL,
    person_id uuid NULL,
    association_id uuid NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    row_version uuid NOT NULL,
    deleted_at timestamp with time zone NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_accounts_email ON accounts (email);
CREATE INDEX IF NOT EXISTS ix_accounts_deleted_at ON accounts (deleted_at);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('202609070001_InitialAuth', '10.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

COMMIT;
