-- =============================================================================
-- AppendOnlyAuditPolicy.sql
-- Run this script MANUALLY in the Supabase SQL editor AFTER applying EF Core
-- migrations. This enforces append-only access on the audit_events table at
-- the PostgreSQL Row Level Security (RLS) level.
-- =============================================================================

-- Enable RLS on audit_events table
ALTER TABLE audit_events ENABLE ROW LEVEL SECURITY;

-- Block all UPDATE operations on audit_events for all roles
CREATE POLICY audit_append_only ON audit_events
    FOR UPDATE USING (false);

-- Block all DELETE operations on audit_events for all roles
CREATE POLICY audit_no_delete ON audit_events
    FOR DELETE USING (false);

-- Allow INSERT for the application role (adjust role name as needed)
-- CREATE POLICY audit_allow_insert ON audit_events
--     FOR INSERT WITH CHECK (true);

-- Allow SELECT for authenticated users
-- CREATE POLICY audit_allow_select ON audit_events
--     FOR SELECT USING (auth.uid() IS NOT NULL);

-- =============================================================================
-- VERIFICATION:
-- After applying this script, verify with:
--   SELECT * FROM pg_policies WHERE tablename = 'audit_events';
-- You should see two policies: audit_append_only and audit_no_delete.
--
-- Unit test note: verify RLS policy blocks UPDATE and DELETE on audit_events
-- at the Supabase level using this script.
-- =============================================================================
