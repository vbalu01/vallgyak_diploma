CREATE SCHEMA `portal`;

CREATE TABLE `portal`.`users` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `email` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `register_date` datetime NOT NULL,
  `status` int NOT NULL
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

CREATE TABLE `portal`.`vehicleOwnerChanges` (
  `id` varchar(255) PRIMARY KEY NOT NULL,
  `vehicle_id` varchar(255) NOT NULL,
  `owner_type` int NOT NULL,
  `new_owner` int NOT NULL,
  `owner_change_date` datetime NOT NULL
);

CREATE TABLE `portal`.`roles` (
  `role` varchar(255) PRIMARY KEY NOT NULL
);

CREATE TABLE `portal`.`userRoles` (
  `userId` int NOT NULL,
  `roleId` varchar(255) NOT NULL,
  PRIMARY KEY (`userId`, `roleId`)
);

CREATE TABLE `portal`.`factories` (
  `id` int PRIMARY KEY NOT NULL,
  `email` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `status` int NOT NULL
);

CREATE TABLE `portal`.`services` (
  `id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `email` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `phone` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `description` varchar(255),
  `status` int NOT NULL
);

CREATE TABLE `portal`.`dealers` (
  `id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `email` varchar(255) NOT NULL,
  `phone` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `description` varchar(255),
  `status` int NOT NULL
);

CREATE TABLE `portal`.`vehiclePermissions` (
  `vehicle_id` varchar(255) NOT NULL,
  `target_id` int NOT NULL,
  `target_type` int NOT NULL,
  `permission` int NOT NULL,
  PRIMARY KEY (`vehicle_id`, `target_id`, `target_type`)
);

CREATE TABLE `portal`.`serviceEvents` (
  `id` varchar(255) PRIMARY KEY NOT NULL,
  `vehicle_id` varchar(255) NOT NULL,
  `service_id` int NOT NULL,
  `title` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `cost` int NOT NULL,
  `date` datetime NOT NULL,
  `mileage` int NOT NULL,
  `serviceType` int NOT NULL,
  `comment` varchar(255)
);

CREATE TABLE `portal`.`crashEvents` (
  `id` varchar(255) PRIMARY KEY NOT NULL,
  `vehicle_id` varchar(255) NOT NULL,
  `date` datetime NOT NULL,
  `mileage` int NOT NULL,
  `description` varchar(255),
  `damageCost` int
);

CREATE TABLE `portal`.`otherCosts` (
  `id` varchar(255) PRIMARY KEY NOT NULL,
  `vehicle_id` varchar(255) NOT NULL,
  `title` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `cost` int NOT NULL,
  `date` datetime NOT NULL
);

CREATE TABLE `portal`.`refuels` (
  `id` varchar(255) PRIMARY KEY NOT NULL,
  `vehicle_id` varchar(255) NOT NULL,
  `refuel_cost` int NOT NULL,
  `traveled_distance` int NOT NULL,
  `amount_of_fuel` int NOT NULL,
  `premium_fuel` bool NOT NULL,
  `fueling_date` datetime NOT NULL
);

CREATE TABLE `portal`.`reviews` (
  `id` varchar(255) PRIMARY KEY NOT NULL,
  `source_type` int NOT NULL,
  `source_id` int NOT NULL,
  `target_type` int NOT NULL,
  `target_id` int NOT NULL,
  `rating` int NOT NULL,
  `description` varchar(255),
  `date` datetime NOT NULL,
  `edited` bool NOT NULL
);

CREATE TABLE `portal`.`saleVehicles` (
  `transaction_id` varchar(255) PRIMARY KEY NOT NULL,
  `vehicle_id` varchar(255) NOT NULL,
  `vehicle_cost` int NOT NULL,
  `phone` varchar(255) NOT NULL,
  `email` varchar(255) NOT NULL,
  `announcement_date` datetime NOT NULL,
  `description` varchar(255) NOT NULL,
  `active` bool NOT NULL
);

CREATE TABLE `portal`.`mileageStands` (
  `id` varchar(255) PRIMARY KEY NOT NULL,
  `vehicle_id` varchar(255) NOT NULL,
  `mileage` int NOT NULL,
  `date` datetime NOT NULL
);

ALTER TABLE `portal`.`vehicleModels` ADD FOREIGN KEY (`make`) REFERENCES `portal`.`vehicleMakes` (`make`);

ALTER TABLE `portal`.`vehicleModels` ADD FOREIGN KEY (`category`) REFERENCES `portal`.`vehicleCategories` (`id`);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`category`) REFERENCES `portal`.`vehicleCategories` (`id`);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`fuel`) REFERENCES `portal`.`fuelTypes` (`id`);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`transmission`) REFERENCES `portal`.`transmissionTypes` (`id`);

ALTER TABLE `portal`.`vehicles` ADD FOREIGN KEY (`drive`) REFERENCES `portal`.`driveTypes` (`id`);

ALTER TABLE `portal`.`vehicleOwnerChanges` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);

ALTER TABLE `portal`.`userRoles` ADD FOREIGN KEY (`userId`) REFERENCES `portal`.`users` (`id`);

ALTER TABLE `portal`.`userRoles` ADD FOREIGN KEY (`roleId`) REFERENCES `portal`.`roles` (`role`);

ALTER TABLE `portal`.`vehiclePermissions` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);

ALTER TABLE `portal`.`serviceEvents` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);

ALTER TABLE `portal`.`serviceEvents` ADD FOREIGN KEY (`service_id`) REFERENCES `portal`.`services` (`id`);

ALTER TABLE `portal`.`crashEvents` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);

ALTER TABLE `portal`.`otherCosts` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);

ALTER TABLE `portal`.`refuels` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);

ALTER TABLE `portal`.`saleVehicles` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);

ALTER TABLE `portal`.`mileageStands` ADD FOREIGN KEY (`vehicle_id`) REFERENCES `portal`.`vehicles` (`chassis_number`);
