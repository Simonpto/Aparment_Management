-- ============================================================
-- Limnos Apartments – Initial Schema
-- ============================================================

-- ---------- apartments ----------
CREATE TABLE apartments (
    id                   UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    title                TEXT        NOT NULL,
    description          TEXT,
    location             TEXT,
    amenities            JSONB       NOT NULL DEFAULT '[]',
    max_guests           SMALLINT,
    bedrooms             SMALLINT,
    base_price_per_night DECIMAL(10,2),
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ---------- apartment_images ----------
CREATE TABLE apartment_images (
    id            UUID    PRIMARY KEY DEFAULT gen_random_uuid(),
    apartment_id  UUID    NOT NULL REFERENCES apartments(id) ON DELETE CASCADE,
    image_url     TEXT    NOT NULL,
    display_order INTEGER NOT NULL DEFAULT 0,
    is_primary    BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_apartment_images_apartment_id ON apartment_images(apartment_id);

-- ---------- availability ----------
CREATE TABLE availability (
    id             UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    apartment_id   UUID        NOT NULL REFERENCES apartments(id) ON DELETE CASCADE,
    date           DATE        NOT NULL,
    is_available   BOOLEAN     NOT NULL DEFAULT TRUE,
    price_override DECIMAL(10,2),
    UNIQUE (apartment_id, date)
);

CREATE INDEX idx_availability_apartment_date ON availability(apartment_id, date);

-- ---------- reviews ----------
CREATE TABLE reviews (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    apartment_id UUID        NOT NULL REFERENCES apartments(id) ON DELETE CASCADE,
    rating       SMALLINT    NOT NULL CHECK (rating BETWEEN 1 AND 5),
    text         TEXT,
    guest_name   TEXT,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    approved     BOOLEAN     NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_reviews_apartment_id ON reviews(apartment_id);

-- ---------- review_tokens ----------
CREATE TABLE review_tokens (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    apartment_id UUID        NOT NULL REFERENCES apartments(id) ON DELETE CASCADE,
    token        UUID        NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    expires_at   TIMESTAMPTZ NOT NULL,
    used         BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_review_tokens_token ON review_tokens(token);

-- ---------- blog_posts ----------
CREATE TABLE blog_posts (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    title        TEXT        NOT NULL,
    slug         TEXT        NOT NULL UNIQUE,
    content      TEXT,
    excerpt      TEXT,
    image_url    TEXT,
    category     TEXT,
    published    BOOLEAN     NOT NULL DEFAULT FALSE,
    published_at TIMESTAMPTZ,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_blog_posts_slug     ON blog_posts(slug);
CREATE INDEX idx_blog_posts_category ON blog_posts(category);

-- ---------- updated_at trigger ----------
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_apartments_updated_at
    BEFORE UPDATE ON apartments
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_blog_posts_updated_at
    BEFORE UPDATE ON blog_posts
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
