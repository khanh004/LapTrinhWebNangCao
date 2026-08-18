CREATE DATABASE student_api_lab
CHARACTER SET utf8mb4
COLLATE utf8mb4_0900_ai_ci;

CREATE USER 'student_api'@'localhost'
IDENTIFIED BY 'ChangeMe_API_2026!';

GRANT ALL PRIVILEGES ON student_api_lab.*
TO 'student_api'@'localhost';

SHOW GRANTS FOR 'student_api'@'localhost';

SHOW DATABASES;