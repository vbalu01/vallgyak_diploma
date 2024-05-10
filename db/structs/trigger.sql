USE portal;

DROP TRIGGER IF EXISTS `MailCheckUsers`;
DROP TRIGGER IF EXISTS `MailCheckDealers`;
DROP TRIGGER IF EXISTS `MailCheckServices`;
DROP TRIGGER IF EXISTS `MailCheckFactories`;

DELIMITER //

CREATE TRIGGER MailCheckUsers
BEFORE INSERT ON users
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;

DELIMITER //

CREATE TRIGGER MailCheckDealers
BEFORE INSERT ON dealers
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;

DELIMITER //

CREATE TRIGGER MailCheckServices
BEFORE INSERT ON services
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;

DELIMITER //

CREATE TRIGGER MailCheckFactories
BEFORE INSERT ON factories
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;

Use portal;

DELIMITER //

CREATE TRIGGER MailCheckUsersUpdate
BEFORE UPDATE ON users
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email AND id <> NEW.id;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;

DELIMITER //

CREATE TRIGGER MailCheckDealersUpdate
BEFORE UPDATE ON dealers
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email AND id <> NEW.id;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;

DELIMITER //

CREATE TRIGGER MailCheckServicesUpdate
BEFORE UPDATE ON services
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email AND id <> NEW.id;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;

DELIMITER //

CREATE TRIGGER MailCheckFactoriesUpdate
BEFORE UPDATE ON factories
FOR EACH ROW
BEGIN
    DECLARE useremailcount INT;
    DECLARE serviceemailcount INT;
    DECLARE dealeremailcount INT;
    DECLARE factoryemailcount INT;
    
    SELECT COUNT(*) INTO useremailcount FROM users WHERE email = NEW.email;
    SELECT COUNT(*) INTO serviceemailcount FROM services WHERE email = NEW.email;
    SELECT COUNT(*) INTO dealeremailcount FROM dealers WHERE email = NEW.email;
    SELECT COUNT(*) INTO factoryemailcount FROM factories WHERE email = NEW.email AND id <> NEW.id;
    
    IF useremailcount > 0 OR serviceemailcount > 0 OR dealeremailcount > 0 OR factoryemailcount >0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Az email cím már használatban van valamelyik táblában!';
    END IF;
    
END//

DELIMITER ;



DELIMITER //
CREATE TRIGGER CheckIfExists
BEFORE INSERT ON vehiclepermissions
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
	IF NEW.target_type = 1 THEN
		SELECT id INTO existsId FROM users WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 2 THEN
		SELECT id INTO existsId FROM factories WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 3 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 4 THEN
		SELECT id INTO existsId FROM services WHERE id = NEW.target_id;
    END IF;
    IF existsId IS NULL OR existsId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;

DELIMITER //
CREATE TRIGGER CheckIfExistsUpdate
BEFORE UPDATE ON vehiclepermissions
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
	IF NEW.target_type = 1 THEN
		SELECT id INTO existsId FROM users WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 2 THEN
		SELECT id INTO existsId FROM factories WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 3 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 4 THEN
		SELECT id INTO existsId FROM services WHERE id = NEW.target_id;
    END IF;
    IF existsId IS NULL OR existsId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;



DELIMITER //
CREATE TRIGGER CheckIfExistsOwnerChanges
BEFORE INSERT ON vehicleownerchanges
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
	IF NEW.owner_type = 1 THEN
		SELECT id INTO existsId FROM users WHERE id = NEW.new_owner;
    END IF;
    IF NEW.owner_type = 2 THEN
		SELECT id INTO existsId FROM factories WHERE id = NEW.new_owner;
    END IF;
    IF NEW.owner_type = 3 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.new_owner;
    END IF;
    IF NEW.owner_type = 4 THEN
		SELECT id INTO existsId FROM services WHERE id = NEW.new_owner;
    END IF;
    IF existsId IS NULL OR existsId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;
DELIMITER //
CREATE TRIGGER CheckIfExistsUpdateOwnerchanges
BEFORE UPDATE ON vehicleownerchanges
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
	IF NEW.owner_type = 1 THEN
		SELECT id INTO existsId FROM users WHERE id = NEW.new_owner;
    END IF;
    IF NEW.owner_type = 2 THEN
		SELECT id INTO existsId FROM factories WHERE id = NEW.new_owner;
    END IF;
    IF NEW.owner_type = 3 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.new_owner;
    END IF;
    IF NEW.owner_type = 4 THEN
		SELECT id INTO existsId FROM services WHERE id = NEW.new_owner;
    END IF;
    IF existsId IS NULL OR existsId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;



DELIMITER //
CREATE TRIGGER CheckIfExistsReview
BEFORE INSERT ON reviews
FOR EACH ROW
BEGIN
    DECLARE sourceId int;
    DECLARE targetId int;
    
	IF NEW.source_type = 1 THEN
		SELECT id INTO sourceId FROM users WHERE id = NEW.source_id;
    END IF;
    IF NEW.source_type = 2 THEN
		SELECT id INTO sourceId FROM factories WHERE id = NEW.source_id;
    END IF;
    IF NEW.source_type = 3 THEN
		SELECT id INTO sourceId FROM dealers WHERE id = NEW.source_id;
    END IF;
    IF NEW.source_type = 4 THEN
		SELECT id INTO sourceId FROM services WHERE id = NEW.source_id;
    END IF;
    
    IF NEW.target_type = 3 THEN
		SELECT id INTO targetId FROM dealers WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 4 THEN
		SELECT id INTO targetId FROM services WHERE id = NEW.target_id;
    END IF;
    
    
    
    IF sourceId IS NULL OR sourceId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett forrás felhasználó nem létezik!';
    END IF;
    IF targetId IS NULL OR targetId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett cél felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;

DELIMITER //
CREATE TRIGGER CheckIfExistsReviewUpdate
BEFORE UPDATE ON reviews
FOR EACH ROW
BEGIN
    DECLARE sourceId int;
    DECLARE targetId int;
    
	IF NEW.source_type = 1 THEN
		SELECT id INTO sourceId FROM users WHERE id = NEW.source_id;
    END IF;
    IF NEW.source_type = 2 THEN
		SELECT id INTO sourceId FROM factories WHERE id = NEW.source_id;
    END IF;
    IF NEW.source_type = 3 THEN
		SELECT id INTO sourceId FROM dealers WHERE id = NEW.source_id;
    END IF;
    IF NEW.source_type = 4 THEN
		SELECT id INTO sourceId FROM services WHERE id = NEW.source_id;
    END IF;
    
    IF NEW.target_type = 3 THEN
		SELECT id INTO targetId FROM dealers WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 4 THEN
		SELECT id INTO targetId FROM services WHERE id = NEW.target_id;
    END IF;
    
    
    
    IF sourceId IS NULL OR sourceId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett forrás felhasználó nem létezik!';
    END IF;
    IF targetId IS NULL OR targetId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett cél felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;


DELIMITER //
CREATE TRIGGER CheckIfExistsReviewRev
BEFORE INSERT ON reviews
FOR EACH ROW
BEGIN
    DECLARE existsId varchar(255);

    SELECT id INTO existsId FROM reviews WHERE source_type = NEW.source_type AND source_id = NEW.source_id AND target_type = NEW.target_type AND target_id = NEW.target_id;
    
    IF existsId IS NOT NULL OR existsId <> "" THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A felhasználó már rögzített véleményt a szolgáltatóhoz!';
    END IF;
END;
//
DELIMITER ;

DELIMITER //
CREATE TRIGGER CheckIfExistsReviewRevUpdate
BEFORE UPDATE ON reviews
FOR EACH ROW
BEGIN
    DECLARE existsId varchar(255);

    SELECT id INTO existsId FROM reviews WHERE source_type = NEW.source_type AND source_id = NEW.source_id AND target_type = NEW.target_type AND target_id = NEW.target_id AND id <> NEW.id;
    
    IF existsId IS NOT NULL OR existsId <> "" THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A felhasználó már rögzített véleményt a szolgáltatóhoz!';
    END IF;
END;
//
DELIMITER ;


DELIMITER //
CREATE TRIGGER CheckIfExistsToken
BEFORE INSERT ON tokens
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
	IF NEW.target_type = 1 THEN
		SELECT id INTO existsId FROM users WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 2 THEN
		SELECT id INTO existsId FROM factories WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 3 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 4 THEN
		SELECT id INTO existsId FROM services WHERE id = NEW.target_id;
    END IF;
    
    IF existsId IS NULL OR existsId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;

DELIMITER //
CREATE TRIGGER CheckIfExistsTokenUpdate
BEFORE UPDATE ON tokens
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
	IF NEW.target_type = 1 THEN
		SELECT id INTO existsId FROM users WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 2 THEN
		SELECT id INTO existsId FROM factories WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 3 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.target_id;
    END IF;
    IF NEW.target_type = 4 THEN
		SELECT id INTO existsId FROM services WHERE id = NEW.target_id;
    END IF;
    
    IF existsId IS NULL OR existsId = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett felhasználó nem létezik!';
    END IF;
END;
//
DELIMITER ;



DELIMITER //
CREATE TRIGGER CheckIfExistsSaleVehicleDealer
BEFORE INSERT ON salevehicles
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
    IF NEW.dealerId <> 0 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.dealerId;
    
		IF existsId IS NULL OR existsId = 0 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett kereskedő nem létezik!';
		END IF;
    END IF;
END;
//
DELIMITER ;

DELIMITER //
CREATE TRIGGER CheckIfExistsSaleVehicleDealerUpdate
BEFORE UPDATE ON salevehicles
FOR EACH ROW
BEGIN
    DECLARE existsId int;
    
    IF NEW.dealerId <> 0 THEN
		SELECT id INTO existsId FROM dealers WHERE id = NEW.dealerId;
    
		IF existsId IS NULL OR existsId = 0 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A keresett kereskedő nem létezik!';
		END IF;
    END IF;
END;
//
DELIMITER ;