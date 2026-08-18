CREATE USER week1_app WITH PASSWORD 'week1_secret';

CREATE DATABASE week1_rbac OWNER week1_app;

SELECT datname
FROM pg_database
WHERE datname = 'week1_rbac';