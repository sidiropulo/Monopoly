 create schema stock;

-- таблица для коробок
CREATE TABLE stock.palletsdb (
    pallet_id SERIAL PRIMARY KEY,
    pallet_name TEXT NOT NULL,
    width FLOAT8 NOT NULL,
    height FLOAT8 NOT NULL,
    depth FLOAT8 NOT NULL,
    weight FLOAT8 NOT NULL DEFAULT 30,
    volume FLOAT8,
    expiry_date DATE null
);

CREATE TABLE stock.boxesdb (
	box_id SERIAL PRIMARY KEY,
	box_name text NOT NULL,
	width float8 NOT NULL,
	height float8 NOT NULL,
	"depth" float8 NOT NULL,
	weight float8 NOT NULL,
	production_date date NOT NULL,
	expiry_date date NULL,
	pallet_id int4 NULL,
	volume float8 NULL,
	CONSTRAINT boxes_pallet_id_fkey FOREIGN KEY (pallet_id) REFERENCES stock.palletsdb(pallet_id) ON DELETE CASCADE
);

CREATE OR REPLACE FUNCTION stock.calculate_box_volume()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
    NEW.volume := NEW.width * NEW.height * NEW.depth;
    RETURN NEW;
END;
$function$
;
-- DROP FUNCTION stock.calculate_box_volume();

CREATE OR REPLACE FUNCTION stock.calculate_box_volume()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
    NEW.volume := NEW.width * NEW.height * NEW.depth;
    RETURN NEW;
END;
$function$
;


-- DROP FUNCTION stock.calculate_pallet_volume();

CREATE OR REPLACE FUNCTION stock.calculate_pallet_volume()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
DECLARE
    pallet_box_volume float8;
    pallet_only_volume float8;
    pallet_new_volume float8;
    min_expiry_date date;
    total_weight float8;
BEGIN
    -- Проверяем операцию (вставка, обновление или удаление)
    CASE
        -- В случае удаления коробки
        WHEN TG_OP = 'DELETE' THEN
            -- Пересчитываем объем паллеты при удалении коробки
            SELECT COALESCE(SUM(volume), 0) INTO pallet_box_volume
            FROM stock.boxesdb
            WHERE pallet_id = OLD.pallet_id;

            SELECT width * height * depth INTO pallet_only_volume
            FROM stock.palletsdb
            WHERE pallet_id = OLD.pallet_id;

            -- Обновляем объем паллеты в таблице palletsdb
            pallet_new_volume = pallet_box_volume + pallet_only_volume;

            -- Обновляем объем и другие атрибуты паллеты
            UPDATE stock.palletsdb
            SET volume = pallet_new_volume
            WHERE pallet_id = OLD.pallet_id;

        -- В случае вставки или обновления коробки
        ELSE
            -- Пересчитываем объем паллеты при вставке или обновлении коробки
            SELECT COALESCE(SUM(volume), 0) INTO pallet_box_volume
            FROM stock.boxesdb
            WHERE pallet_id = NEW.pallet_id;

            SELECT width * height * depth INTO pallet_only_volume
            FROM stock.palletsdb
            WHERE pallet_id = NEW.pallet_id;

            -- Обновляем объем паллеты в таблице palletsdb
            pallet_new_volume = pallet_box_volume + pallet_only_volume;

            -- Обновляем объем и другие атрибуты паллеты
            UPDATE stock.palletsdb
            SET volume = pallet_new_volume
            WHERE pallet_id = NEW.pallet_id;
    END CASE;

    -- Вычисляем минимальный срок годности коробок, вложенных в паллету
    SELECT MIN(expiry_date) INTO min_expiry_date
    FROM stock.boxesdb
    WHERE pallet_id = NEW.pallet_id;

    -- Обновляем срок годности паллеты в таблице palletsdb
    UPDATE stock.palletsdb
    SET expiry_date = min_expiry_date
    WHERE pallet_id = NEW.pallet_id;

    -- Вычисляем суммарный вес вложенных коробок и добавляем 30 кг
    SELECT COALESCE(SUM(weight), 0) INTO total_weight
    FROM stock.boxesdb
    WHERE pallet_id = NEW.pallet_id;
    total_weight := total_weight + 30;

    -- Обновляем вес паллеты в таблице palletsdb
    UPDATE stock.palletsdb
    SET weight = total_weight
    WHERE pallet_id = NEW.pallet_id;

    RETURN NEW;
END;
$function$
;

-- DROP FUNCTION stock.check_box_size();

CREATE OR REPLACE FUNCTION stock.check_box_size()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
DECLARE
    pallet_name text;
    pallet_width float8;
    pallet_depth float8;
BEGIN
    -- Получаем имя и размеры паллеты
    SELECT palletsdb.pallet_name, palletsdb.width, palletsdb.depth INTO pallet_name, pallet_width, pallet_depth
    FROM stock.palletsdb
    WHERE palletsdb.pallet_id = NEW.pallet_id;

    -- Проверяем, не превышают ли размеры коробки размеры паллеты по ширине и глубине
    IF NEW.width > pallet_width OR NEW.depth > pallet_depth THEN
        RAISE EXCEPTION 'Коробка "%", не помещается в паллету "%"', NEW.box_name, pallet_name;
    END IF;

    RETURN NEW;
END;
$function$
;

-- DROP FUNCTION stock.set_expiry_date();

CREATE OR REPLACE FUNCTION stock.set_expiry_date()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
    NEW.expiry_date := NEW.production_date + INTERVAL '100 days';
    RETURN NEW;
END;
$function$
;


-- DROP FUNCTION stock.insertbox(text, text, float8, float8, float8, float8, date);

CREATE OR REPLACE FUNCTION stock.insertbox(_box_name text, _pallet_name text, width double precision, height double precision, depth double precision, weight double precision, _production_date date)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
DECLARE
    _pallet_id bigint;
BEGIN
    -- Получаем идентификатор паллеты по имени
    SELECT pallet_id INTO _pallet_id FROM stock.palletsdb WHERE pallet_name = _pallet_name;

    -- Проверяем, существует ли паллета с указанным именем
    IF _pallet_id IS NULL THEN
        RAISE EXCEPTION 'Паллета с именем % не существует', _pallet_name;
    END IF;

    -- Вставляем новую коробку в указанную паллету
    INSERT INTO stock.boxesdb  (box_name, pallet_id, width, height, depth, weight, production_date)
    VALUES (_box_name, _pallet_id, width, height, depth, weight, _production_date);
END;
$function$
;


-- DROP FUNCTION stock.insertpallet(text, float8, float8, float8);

CREATE OR REPLACE FUNCTION stock.insertpallet(_pallet_name text, width double precision, height double precision, depth double precision)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
DECLARE
    pallet_exists BOOLEAN;
BEGIN
    -- Проверяем, существует ли паллета с таким именем
    SELECT EXISTS(SELECT 1 FROM stock.palletsdb WHERE pallet_name = _pallet_name) INTO pallet_exists;
    
    -- Если паллета с таким именем уже существует, выводим сообщение
    IF pallet_exists THEN
        RAISE EXCEPTION 'Паллета с именем % уже существует', pallet_name;
    ELSE
        -- Вставляем новую паллету
        INSERT INTO stock.palletsdb (pallet_name, width, height, depth)
        VALUES (_pallet_name, width, height, depth);
    END IF;
END;
$function$
;


create trigger calculate_box_volume_trigger before
insert
    on
    stock.boxesdb for each row execute function stock.calculate_box_volume();

create trigger calculate_pallet_volume_trigger after
insert
    or
delete
    on
    stock.boxesdb for each row execute function stock.calculate_pallet_volume();

create trigger check_box_size_trigger before
insert
    on
    stock.boxesdb for each row execute function stock.check_box_size();

create trigger set_expiry_date_trigger before
insert
    on
    stock.boxesdb for each row execute function stock.set_expiry_date();





    DO $$
DECLARE
    pallet_name text;
    width float8;
    height float8;
    depth float8;
    box_name text;
    box_width float8;
    box_height float8;
    box_depth float8;
    box_weight float8;
    production_date date;
BEGIN
    FOR i IN 1..25 LOOP -- Изменено количество палеток на 25
        -- Генерация случайных параметров для паллеты
        pallet_name := 'Pallet_' || i::text;
        width := ROUND(random() * 40 + 80)::numeric::float8; -- Ширина от 80 до 120 с двумя знаками после запятой
        height := ROUND(random() * 40 + 80)::numeric::float8; -- Высота от 80 до 120 с двумя знаками после запятой
        depth := ROUND(random() * 40 + 80)::numeric::float8; -- Глубина от 80 до 120 с двумя знаками после запятой
        
        -- Вставка паллеты
        PERFORM stock.insertpallet(pallet_name, width, height, depth);
        
        -- Генерация случайного количества коробок от 4 до 6
        FOR j IN 1..(random() * 3 + 4) LOOP
            -- Генерация случайных параметров для коробки
            box_name := 'Box_' || i::text || '_' || j::text;
            box_width := ROUND(random() * (width - 20) + 10)::numeric::float8; -- Ширина коробки от 10 до width - 10 с двумя знаками после запятой
            box_height := ROUND(random() * (height - 20) + 10)::numeric::float8; -- Высота коробки от 10 до height - 10 с двумя знаками после запятой
            box_depth := ROUND(random() * (depth - 20) + 10)::numeric::float8; -- Глубина коробки от 10 до depth - 10 с двумя знаками после запятой
            box_weight := ROUND(random() * 45 + 10)::numeric::float8; -- Вес коробки от 10 до 55 с двумя знаками после запятой
            
            -- Генерация случайной даты с месяцем февраль и годом от 2022 до 2024
            production_date := to_date('02-04-' || (2023 + ROUND(random() * 2))::text, 'DD-MM-YYYY');
            
            -- Вставка коробки
            PERFORM stock.insertbox(box_name, pallet_name, box_width, box_height, box_depth, box_weight, production_date);
        END LOOP;
    END LOOP;
END $$;
