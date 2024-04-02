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