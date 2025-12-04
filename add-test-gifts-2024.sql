-- Script pour ajouter des cadeaux de test pour Sylvain en 2024

-- D'abord, trouver l'ID de Sylvain et sa liste
-- Supposons que Sylvain a l'ID 2 (à ajuster si nécessaire)

-- Insérer un cadeau libre (disponible) pour 2024
INSERT INTO gifts (list_id, name, description, link, image, cost, currency, available, taken_by, is_group_gift, year, created_at, updated_at)
SELECT
    l.id,
    'Livre de science-fiction',
    'Un bon roman de science-fiction, idéalement de la série Fondation d''Asimov',
    'https://www.amazon.fr/Fondation-Isaac-Asimov/dp/2070360539',
    'https://m.media-amazon.com/images/I/51VVQGjZr5L._SY445_SX342_.jpg',
    15.90,
    'EUR',
    1, -- available
    NULL, -- pas réservé
    0, -- pas un cadeau groupé
    2024,
    datetime('now'),
    datetime('now')
FROM lists l
JOIN users u ON l.user_id = u.id
WHERE u.login = 'sylvain';

-- Insérer un cadeau réservé par un autre utilisateur pour 2024
INSERT INTO gifts (list_id, name, description, link, image, cost, currency, available, taken_by, is_group_gift, year, created_at, updated_at)
SELECT
    l.id,
    'Casque audio Bluetooth',
    'Un casque audio de bonne qualité avec réduction de bruit',
    'https://www.amazon.fr/Sony-WH-1000XM4-Bluetooth-R%C3%A9duction-Argent/dp/B08C7KG5LP',
    'https://m.media-amazon.com/images/I/71o8Q5XJS5L._AC_SX679_.jpg',
    279.00,
    'EUR',
    0, -- pas disponible
    (SELECT id FROM users WHERE login = 'nawel' LIMIT 1), -- réservé par nawel
    0, -- pas un cadeau groupé
    2024,
    datetime('now'),
    datetime('now')
FROM lists l
JOIN users u ON l.user_id = u.id
WHERE u.login = 'sylvain';
