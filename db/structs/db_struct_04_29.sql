CREATE SCHEMA `portal`;

CREATE TABLE `portal`.`users` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `email` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `mail_confirmed` boolean NOT NULL,
  `enabled` boolean NOT NULL,
  `register_date` datetime NOT NULL
);

CREATE TABLE `portal`.`vehicleCategories` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `category` varchar(255) NOT NULL
);

CREATE TABLE `portal`.`vehicleMakes` (
  `make` varchar(255) PRIMARY KEY NOT NULL
);

CREATE TABLE `portal`.`vehicleModels` (
  `model` varchar(255) NOT NULL,
  `make` varchar(255) NOT NULL,
  `category` int NOT NULL,
  PRIMARY KEY (`model`, `make`)
);

CREATE TABLE `portal`.`fuelTypes` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `fuel` varchar(255) NOT NULL
);

CREATE TABLE `portal`.`transmissionTypes` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `transmission` varchar(255) NOT NULL
);

CREATE TABLE `portal`.`driveTypes` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `drive` varchar(255) NOT NULL
);

CREATE TABLE `portal`.`bodyTypes` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `body` varchar(255) NOT NULL
);

CREATE TABLE `portal`.`vehicles` (
  `chassis_number` varchar(255) PRIMARY KEY NOT NULL,
  `engine_number` varchar(255) UNIQUE NOT NULL,
  `license` varchar(255),
  `engine_code` varchar(255) NOT NULL,
  `category` int NOT NULL,
  `manufact_year` int NOT NULL,
  `make` varchar(255) NOT NULL,
  `model` varchar(255) NOT NULL,
  `modeltype` varchar(255),
  `fuel` int NOT NULL,
  `transmission` int NOT NULL,
  `drive` int NOT NULL,
  `engine_ccm` int,
  `performance` int NOT NULL,
  `torque` int NOT NULL,
  `body` varchar(255),
  `num_of_doors` int NOT NULL,
  `num_of_seats` int NOT NULL,
  `weight` int NOT NULL,
  `max_weight` int NOT NULL
);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`category`) REFERENCES `portal`.`vehicleCategories` (`id`);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`fuel`) REFERENCES `portal`.`fuelTypes` (`id`);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`transmission`) REFERENCES `portal`.`transmissionTypes` (`id`);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`drive`) REFERENCES `portal`.`driveTypes` (`id`);

ALTER TABLE `portal`.`vehicleModels` ADD FOREIGN KEY (`category`) REFERENCES `portal`.`vehicleCategories` (`id`);
