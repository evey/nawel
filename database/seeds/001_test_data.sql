-- Test data for development
-- This file creates test users and families

-- Insert test families
INSERT INTO family (id, name, created_at) VALUES
(1, 'Nironi', datetime('now')),
(2, 'Elfassi', datetime('now'));

-- Insert test users
-- Password for all test users is "password123" (will be hashed as MD5 initially, then migrated to BCrypt on first login)
-- MD5 hash of "password123" = 482c811da5d5b4bc6d497ffa98491e38

INSERT INTO user (id, login, pwd, email, first_name, last_name, avatar, pseudo, notify_list_edit, notify_gift_taken, display_popup, isChildren, family_id, created_at, updated_at) VALUES
(1, 'admin', '21232f297a57a5a743894a0e4a801fc3', 'admin@nawel.com', 'Admin', 'System', 'avatar.png', 'Admin', 0, 0, 1, 0, 1, datetime('now'), datetime('now')),
(2, 'sylvain', '482c811da5d5b4bc6d497ffa98491e38', 'sylvain@nawel.com', 'Sylvain', 'Nironi', 'avatar.png', 'Sylvain', 1, 1, 1, 0, 1, datetime('now'), datetime('now')),
(3, 'claire', '482c811da5d5b4bc6d497ffa98491e38', 'claire@nawel.com', 'Claire', 'Nironi', 'avatar.png', 'Claire', 1, 0, 1, 0, 1, datetime('now'), datetime('now')),
(4, 'marie', '482c811da5d5b4bc6d497ffa98491e38', 'marie@nawel.com', 'Marie', 'Nironi', 'avatar.png', 'Marie', 1, 0, 1, 0, 1, datetime('now'), datetime('now'));

-- Create lists for users
INSERT INTO lists (id, name, user_id, created_at, updated_at) VALUES
(1, 'Liste de Sylvain', 2, datetime('now'), datetime('now')),
(2, 'Liste Admin', 1, datetime('now'), datetime('now')),
(3, 'Liste de Claire', 3, datetime('now'), datetime('now')),
(4, 'Liste de Marie', 4, datetime('now'), datetime('now'));

-- Insert some test gifts for year 2025
INSERT INTO gifts (id, list_id, name, description, image, link, cost, currency, available, taken_by, comment, year, created_at, updated_at) VALUES
(1, 1, 'Livre de cuisine', 'Un beau livre de recettes', 'https://via.placeholder.com/150', 'https://example.com', 29.99, 'EUR', 1, NULL, NULL, 2025, datetime('now'), datetime('now')),
(2, 1, 'Casque audio', 'Casque Bluetooth', 'https://via.placeholder.com/150', 'https://example.com', 79.99, 'EUR', 1, NULL, NULL, 2025, datetime('now'), datetime('now')),
(3, 3, 'Sac à main', 'Sac élégant', 'https://via.placeholder.com/150', 'https://example.com', 120.00, 'EUR', 1, NULL, NULL, 2025, datetime('now'), datetime('now')),
(4, 4, 'Parfum', 'Eau de toilette', 'https://via.placeholder.com/150', 'https://example.com', 65.00, 'EUR', 1, NULL, NULL, 2025, datetime('now'), datetime('now'));
