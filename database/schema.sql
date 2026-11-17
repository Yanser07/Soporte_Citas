IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102135510_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Correo] nvarchar(150) NOT NULL,
        [Telefono] nvarchar(30) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Rol] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102135510_InitialCreate'
)
BEGIN
    CREATE TABLE [Citas] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] int NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [TipoServicio] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(1000) NOT NULL,
        [Estado] int NOT NULL,
        [MotivoRespuesta] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaRespuesta] datetime2 NULL,
        CONSTRAINT [PK_Citas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Citas_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102135510_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Citas_FechaHora] ON [Citas] ([FechaHora]) WHERE [Estado] IN (1, 2)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102135510_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Citas_UsuarioId] ON [Citas] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102135510_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Correo] ON [Usuarios] ([Correo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102135510_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261102135510_InitialCreate', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110202616_AgregarBloqueoLogin'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [BloqueadoHasta] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110202616_AgregarBloqueoLogin'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [IntentosFallidos] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110202616_AgregarBloqueoLogin'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110202616_AgregarBloqueoLogin', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110230326_AgregarRecuperacionPassword'
)
BEGIN
    CREATE TABLE [RecuperacionesPassword] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] int NOT NULL,
        [TokenHash] nvarchar(64) NOT NULL,
        [Expira] datetime2 NOT NULL,
        [UsadoEn] datetime2 NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_RecuperacionesPassword] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecuperacionesPassword_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110230326_AgregarRecuperacionPassword'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RecuperacionesPassword_TokenHash] ON [RecuperacionesPassword] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110230326_AgregarRecuperacionPassword'
)
BEGIN
    CREATE INDEX [IX_RecuperacionesPassword_UsuarioId] ON [RecuperacionesPassword] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110230326_AgregarRecuperacionPassword'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110230326_AgregarRecuperacionPassword', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111005444_AgregarHorarioYBloqueos'
)
BEGIN
    CREATE TABLE [BloqueosHorario] (
        [Id] int NOT NULL IDENTITY,
        [FechaInicio] date NOT NULL,
        [FechaFin] date NOT NULL,
        [HoraDesde] int NULL,
        [HoraHasta] int NULL,
        [Motivo] nvarchar(200) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_BloqueosHorario] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111005444_AgregarHorarioYBloqueos'
)
BEGIN
    CREATE TABLE [ConfiguracionesHorario] (
        [Id] int NOT NULL IDENTITY,
        [DiasLaborables] nvarchar(20) NOT NULL,
        [HoraInicio] int NOT NULL,
        [HoraFin] int NOT NULL,
        CONSTRAINT [PK_ConfiguracionesHorario] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111005444_AgregarHorarioYBloqueos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261111005444_AgregarHorarioYBloqueos', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111170225_AgregarModalidadYCompletada'
)
BEGIN
    ALTER TABLE [Citas] ADD [Direccion] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111170225_AgregarModalidadYCompletada'
)
BEGIN
    ALTER TABLE [Citas] ADD [FechaCompletada] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111170225_AgregarModalidadYCompletada'
)
BEGIN
    ALTER TABLE [Citas] ADD [Modalidad] int NOT NULL DEFAULT 2;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111170225_AgregarModalidadYCompletada'
)
BEGIN
    ALTER TABLE [Citas] ADD [NotasInternas] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111170225_AgregarModalidadYCompletada'
)
BEGIN
    ALTER TABLE [Citas] ADD [ResumenTrabajo] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111170225_AgregarModalidadYCompletada'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261111170225_AgregarModalidadYCompletada', N'10.0.14');
END;

COMMIT;
GO

