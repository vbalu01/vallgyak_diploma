CREATE SCHEMA `portal`;

CREATE TABLE `portal`.`users` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `email` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `mail_confirmed` boolean NOT NULL DEFAULT FALSE,
  `enabled` boolean NOT NULL DEFAULT TRUE,
  `register_date` datetime NOT NULL DEFAULT NOW()
);
