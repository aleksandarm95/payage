-- 001_init.sql

-- UUID generator
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =========================
-- transactions (current state)
-- =========================
CREATE TABLE IF NOT EXISTS transactions (
    id                  uuid            PRIMARY KEY DEFAULT gen_random_uuid(),
    order_reference     varchar(50)     NOT NULL,
    status              text            NOT NULL,
    amount              numeric(18,2)   NOT NULL,
    currency            char(3)         NOT NULL,
    masked_card_number  varchar(32)     NOT NULL,
    cardholder_name     varchar(100)    NOT NULL,
    captured_amount     numeric(18,2)   NOT NULL DEFAULT 0,
    refunded_amount     numeric(18,2)   NOT NULL DEFAULT 0,
    created_at          timestamptz     NOT NULL DEFAULT now(),
    updated_at          timestamptz     NOT NULL DEFAULT now(),
    row_version         int             NOT NULL DEFAULT 1,

    CONSTRAINT order_reference_unique UNIQUE (order_reference),

    CONSTRAINT status_check
        CHECK (status IN ('AUTHORIZED', 'CAPTURED', 'VOIDED', 'REFUNDED')),

    CONSTRAINT amount_positive_check
        CHECK (amount > 0),

    CONSTRAINT captured_amount_range_check
        CHECK (captured_amount >= 0 AND captured_amount <= amount),

    CONSTRAINT refunded_amount_range_check
        CHECK (refunded_amount >= 0 AND refunded_amount <= captured_amount)
);

-- Helpful index for listing/filtering
CREATE INDEX IF NOT EXISTS status_created_at_index
    ON transactions (status, created_at DESC);

-- =========================
-- transaction_events (audit trail)
-- =========================
CREATE TABLE IF NOT EXISTS transaction_events (
    id              bigserial       PRIMARY KEY,
    transaction_id  uuid            NOT NULL,
    event_type      text            NOT NULL,
    amount          numeric(18,2)   NULL,
    reason          text            NULL,
    created_at      timestamptz     NOT NULL DEFAULT now(),

    CONSTRAINT transaction_id_fk
        FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE CASCADE,

    CONSTRAINT event_type_check
        CHECK (event_type IN ('AUTHORIZED', 'CAPTURED', 'VOIDED', 'REFUNDED'))
);

CREATE INDEX IF NOT EXISTS transaction_id_created_at_index
    ON transaction_events (transaction_id, created_at DESC);
