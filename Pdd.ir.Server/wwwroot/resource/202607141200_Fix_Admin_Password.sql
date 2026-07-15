-- Fix admin password hash (password: admin123)
-- SHA256 hash = JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=
UPDATE Users SET PasswordHash = N'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=' WHERE Username = N'admin';
