-- Create public storage bucket for apartment images
INSERT INTO storage.buckets (id, name, public)
VALUES ('apartments', 'apartments', true)
ON CONFLICT (id) DO NOTHING;
