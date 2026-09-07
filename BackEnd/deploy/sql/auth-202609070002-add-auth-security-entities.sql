-- GoCare Auth - migrazione 202609070002_AddAuthSecurityEntities
-- Eseguire in gocare_auth negli ambienti non Development.

CREATE TABLE email_verification_tokens (
    id uuid NOT NULL,
    account_id uuid NOT NULL,
    token character varying(512) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    used_at timestamp with time zone NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    row_version uuid NOT NULL,
    deleted_at timestamp with time zone NULL,
    CONSTRAINT pk_email_verification_tokens PRIMARY KEY (id),
    CONSTRAINT fk_email_verification_tokens_accounts_account_id FOREIGN KEY (account_id)
        REFERENCES accounts (id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX ix_email_verification_tokens_token ON email_verification_tokens (token);
CREATE INDEX ix_email_verification_tokens_account_id_expires_at ON email_verification_tokens (account_id, expires_at);
CREATE INDEX ix_email_verification_tokens_deleted_at ON email_verification_tokens (deleted_at);

CREATE TABLE password_reset_tokens (
    id uuid NOT NULL,
    account_id uuid NOT NULL,
    token character varying(512) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    used_at timestamp with time zone NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    row_version uuid NOT NULL,
    deleted_at timestamp with time zone NULL,
    CONSTRAINT pk_password_reset_tokens PRIMARY KEY (id),
    CONSTRAINT fk_password_reset_tokens_accounts_account_id FOREIGN KEY (account_id)
        REFERENCES accounts (id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX ix_password_reset_tokens_token ON password_reset_tokens (token);
CREATE INDEX ix_password_reset_tokens_account_id_expires_at ON password_reset_tokens (account_id, expires_at);
CREATE INDEX ix_password_reset_tokens_deleted_at ON password_reset_tokens (deleted_at);

CREATE TABLE refresh_tokens (
    id uuid NOT NULL,
    account_id uuid NOT NULL,
    token character varying(512) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    revoked_at timestamp with time zone NULL,
    user_agent character varying(512) NULL,
    ip_address character varying(64) NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    row_version uuid NOT NULL,
    deleted_at timestamp with time zone NULL,
    CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
    CONSTRAINT fk_refresh_tokens_accounts_account_id FOREIGN KEY (account_id)
        REFERENCES accounts (id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX ix_refresh_tokens_token ON refresh_tokens (token);
CREATE INDEX ix_refresh_tokens_account_id_expires_at ON refresh_tokens (account_id, expires_at);
CREATE INDEX ix_refresh_tokens_deleted_at ON refresh_tokens (deleted_at);

CREATE TABLE failed_login_attempts (
    id uuid NOT NULL,
    account_id uuid NULL,
    email character varying(320) NOT NULL,
    attempted_at timestamp with time zone NOT NULL,
    ip_address character varying(64) NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    row_version uuid NOT NULL,
    deleted_at timestamp with time zone NULL,
    CONSTRAINT pk_failed_login_attempts PRIMARY KEY (id),
    CONSTRAINT fk_failed_login_attempts_accounts_account_id FOREIGN KEY (account_id)
        REFERENCES accounts (id) ON DELETE SET NULL
);
CREATE INDEX ix_failed_login_attempts_account_id_attempted_at ON failed_login_attempts (account_id, attempted_at);
CREATE INDEX ix_failed_login_attempts_email_attempted_at ON failed_login_attempts (email, attempted_at);
CREATE INDEX ix_failed_login_attempts_deleted_at ON failed_login_attempts (deleted_at);
