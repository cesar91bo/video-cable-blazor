-- sqlcmd -S .\SQLEXPRESS -E -b -i scripts\MigrarClientes.sql -v Aplicar=0
-- Aplicar=0: simulación con ROLLBACK. Aplicar=1: respaldo y COMMIT.
USE VideoCableDb;
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
DECLARE @Aplicar bit = $(Aplicar);
IF DB_ID('SgPymeBase') IS NULL THROW 50000, 'No existe SgPymeBase.', 1;
IF @Aplicar = 1
BEGIN
    DECLARE @Ruta nvarchar(4000), @Respaldo nvarchar(4000);
    SELECT @Ruta = LEFT(physical_name, LEN(physical_name) - CHARINDEX('\', REVERSE(physical_name)) + 1)
    FROM sys.database_files WHERE file_id = 1;
    SET @Respaldo = @Ruta + 'VideoCableDb_antes_clientes_' + CONVERT(char(8), GETDATE(), 112)
        + '_' + REPLACE(CONVERT(char(8), GETDATE(), 108), ':', '') + '.bak';
    BACKUP DATABASE VideoCableDb TO DISK = @Respaldo WITH COPY_ONLY, CHECKSUM;
    RESTORE VERIFYONLY FROM DISK = @Respaldo WITH CHECKSUM;
    SELECT @Respaldo AS RespaldoVerificado;
END;

BEGIN TRY
BEGIN TRANSACTION;
-- Impide que una alta concurrente invalide la comparación o los códigos nuevos.
DECLARE @CantidadDestino int;
SELECT @CantidadDestino = COUNT(*) FROM dbo.Clientes WITH (TABLOCKX, HOLDLOCK);
SELECT Id INTO #Demos FROM dbo.Clientes WHERE Nombre LIKE N'DEMO - %' AND IsDeleted = 0;
UPDATE s SET EstadoActual=2, FechaBaja=COALESCE(FechaBaja,GETDATE()), IsDeleted=1,
    DeletedAt=GETDATE(), UpdatedAt=GETDATE()
FROM dbo.Suscripciones s JOIN #Demos d ON s.ClienteId=d.Id;
DECLARE @SuscripcionesDemo int = @@ROWCOUNT;
UPDATE c SET Activo=0, IsDeleted=1, DeletedAt=GETDATE(), UpdatedAt=GETDATE()
FROM dbo.Clientes c JOIN #Demos d ON c.Id=d.Id;

SELECT c.NroCliente AS CodigoOrigen, LTRIM(RTRIM(c.ApellidoyNombre)) AS Nombre,
    UPPER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(c.ApellidoyNombre,CHAR(9),' '),CHAR(10),' '),CHAR(13),' ')))) COLLATE Latin1_General_100_CI_AI AS NombreClave,
    CASE UPPER(LTRIM(RTRIM(c.TipoDocumento))) WHEN 'DNI' THEN 1 WHEN 'CUIT' THEN 2 WHEN 'CUIL' THEN 3 WHEN 'LE' THEN 4 WHEN 'LC' THEN 5 ELSE 0 END AS TipoDocumento,
    CASE WHEN c.NroDocumento > 0 THEN CONVERT(nvarchar(20),c.NroDocumento) END AS Documento,
    NULLIF(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(CONCAT(c.Cuit0,c.Cuit1,c.Cuit2))),'-',''),' ',''),'.',''),'') AS Cuit,
    c.IdRegimenImpositivo AS RegimenImpositivo, c.Direccion, p.Localidad,
    LEFT(NULLIF(LTRIM(RTRIM(c.Telefono)),''),50) AS Telefono,
    COALESCE(NULLIF(LTRIM(RTRIM(c.Email1)),''),NULLIF(LTRIM(RTRIM(c.Email2)),'')) AS Email,
    LEFT(CONCAT('Migrado de SgPymeBase; código original: ',c.NroCliente,
        CASE WHEN LEN(c.Telefono)>50 THEN CONCAT('; teléfono completo: ',c.Telefono) ELSE '' END,
        CASE WHEN NULLIF(c.Comentario,'') IS NOT NULL THEN CONCAT('; ',c.Comentario) ELSE '' END,
        CASE WHEN NULLIF(c.MensajeCuenta,'') IS NOT NULL THEN CONCAT('; ',c.MensajeCuenta) ELSE '' END),1000) AS Observaciones,
    CONVERT(bit,CASE WHEN c.FechaBaja IS NULL THEN 1 ELSE 0 END) AS Activo
INTO #Origen FROM SgPymeBase.dbo.Clientes c
LEFT JOIN SgPymeBase.dbo.CPostales p ON p.CodigoPostal=c.CodigoPostal AND p.SubCodigoPostal=c.SubCodigoPostal;
WHILE EXISTS(SELECT 1 FROM #Origen WHERE NombreClave LIKE '%  %')
    UPDATE #Origen SET NombreClave=REPLACE(NombreClave,'  ',' ');
UPDATE #Origen SET Cuit=NULL WHERE Cuit IS NOT NULL AND REPLACE(Cuit,'0','')='';
-- El sistema viejo guarda los CUIT en tres campos, no en NroDocumento.
UPDATE #Origen SET Documento=Cuit WHERE TipoDocumento IN (2,3) AND Cuit IS NOT NULL;

CREATE TABLE #Claves(Nombre nvarchar(200) COLLATE Latin1_General_100_CI_AI,
    Documento nvarchar(20), Cuit nvarchar(11), Referencia nvarchar(80));
INSERT #Claves
SELECT UPPER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(Nombre,CHAR(9),' '),CHAR(10),' '),CHAR(13),' ')))) COLLATE Latin1_General_100_CI_AI,
    CASE WHEN TRY_CONVERT(bigint,REPLACE(REPLACE(REPLACE(NumeroDocumento,'.',''),'-',''),' ',''))>0
        THEN CONVERT(nvarchar(20),TRY_CONVERT(bigint,REPLACE(REPLACE(REPLACE(NumeroDocumento,'.',''),'-',''),' ',''))) END,
    NULLIF(REPLACE(REPLACE(REPLACE(Cuit,'-',''),' ',''),'.',''),''),CONCAT('Destino Id=',Id)
FROM dbo.Clientes; -- También compara los eliminados: sus documentos siguen teniendo índices únicos.
WHILE EXISTS(SELECT 1 FROM #Claves WHERE Nombre LIKE '%  %') UPDATE #Claves SET Nombre=REPLACE(Nombre,'  ',' ');
CREATE TABLE #Resultado(CodigoOrigen int, Nombre nvarchar(200), Accion nvarchar(30), Motivo nvarchar(200), CodigoDestino int NULL);
DECLARE @Codigo int, @Nombre nvarchar(200), @Clave nvarchar(200), @Doc nvarchar(20), @Cuit nvarchar(50),
    @Tipo int, @Regimen int, @Motivo nvarchar(200), @NuevoCodigo int, @Insertados int=0;
SELECT @NuevoCodigo=ISNULL(MAX(Codigo),0) FROM dbo.Clientes;
DECLARE clientes CURSOR LOCAL FAST_FORWARD FOR
    SELECT CodigoOrigen,Nombre,NombreClave,Documento,Cuit,TipoDocumento,RegimenImpositivo FROM #Origen ORDER BY CodigoOrigen;
OPEN clientes;
FETCH NEXT FROM clientes INTO @Codigo,@Nombre,@Clave,@Doc,@Cuit,@Tipo,@Regimen;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @Motivo=NULL;
    IF NULLIF(@Nombre,'') IS NULL SET @Motivo=N'Nombre vacío';
    ELSE IF @Regimen NOT BETWEEN 1 AND 6 SET @Motivo=N'Condición IVA desconocida';
    ELSE IF @Cuit IS NOT NULL AND (LEN(@Cuit)<>11 OR @Cuit LIKE '%[^0-9]%') SET @Motivo=N'CUIT con formato inválido';
    ELSE IF @Doc IS NOT NULL AND @Tipo=0 SET @Motivo=N'Tipo de documento desconocido';
    IF @Motivo IS NOT NULL
        INSERT #Resultado VALUES(@Codigo,@Nombre,'REVISAR',@Motivo,NULL);
    ELSE
    BEGIN
        SELECT TOP(1) @Motivo=CONCAT(CASE WHEN Nombre=@Clave COLLATE Latin1_General_100_CI_AI THEN 'Nombre' WHEN Documento=@Doc THEN 'Documento' ELSE 'CUIT' END,' coincide con ',Referencia)
        FROM #Claves WHERE Nombre=@Clave COLLATE Latin1_General_100_CI_AI
            OR (@Doc IS NOT NULL AND Documento=@Doc) OR (@Cuit IS NOT NULL AND Cuit=@Cuit)
            OR (@Doc IS NOT NULL AND Cuit=@Doc) OR (@Cuit IS NOT NULL AND Documento=@Cuit)
        ORDER BY Referencia;
        IF @Motivo IS NOT NULL
            INSERT #Resultado VALUES(@Codigo,@Nombre,'OMITIDO',@Motivo,NULL);
        ELSE
        BEGIN
            SET @NuevoCodigo+=1;
            INSERT dbo.Clientes(Codigo,Nombre,TipoDocumento,NumeroDocumento,Cuit,RegimenImpositivo,Direccion,Localidad,Telefono,Email,Observaciones,Activo,CreatedAt,IsDeleted)
            SELECT @NuevoCodigo,Nombre,CASE WHEN Documento IS NULL THEN 0 ELSE TipoDocumento END,Documento,Cuit,RegimenImpositivo,Direccion,Localidad,Telefono,Email,Observaciones,Activo,GETDATE(),0
            FROM #Origen WHERE CodigoOrigen=@Codigo;
            INSERT #Claves VALUES(@Clave,@Doc,@Cuit,CONCAT('Origen código=',@Codigo));
            INSERT #Resultado VALUES(@Codigo,@Nombre,'IMPORTAR',NULL,@NuevoCodigo);
            SET @Insertados+=1;
        END;
    END;
    FETCH NEXT FROM clientes INTO @Codigo,@Nombre,@Clave,@Doc,@Cuit,@Tipo,@Regimen;
END;
CLOSE clientes; DEALLOCATE clientes;
SELECT CASE WHEN @Aplicar=1 THEN 'APLICADO' ELSE 'SIMULACION (ROLLBACK)' END AS Modo,
    (SELECT COUNT(*) FROM #Origen) AS Origen, @Insertados AS Importados,
    (SELECT COUNT(*) FROM #Resultado WHERE Accion='OMITIDO') AS Omitidos,
    (SELECT COUNT(*) FROM #Resultado WHERE Accion='REVISAR') AS Revisar,
    (SELECT COUNT(*) FROM #Demos) AS DemosDadosDeBaja, @SuscripcionesDemo AS SuscripcionesDemoDadasDeBaja;
SELECT * FROM #Resultado ORDER BY Accion,CodigoOrigen;
IF @Aplicar=1 COMMIT; ELSE ROLLBACK;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
