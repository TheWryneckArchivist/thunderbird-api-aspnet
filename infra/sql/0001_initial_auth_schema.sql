CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE TABLE "DiscordOAuthStates" (
        "Id" uuid NOT NULL,
        "State" character varying(64) NOT NULL,
        "RedirectUri" character varying(1024) NOT NULL,
        "Platform" integer NOT NULL,
        "CreatedAtUnix" bigint NOT NULL,
        "ExpiresAtUnix" bigint NOT NULL,
        "ConsumedAtUnix" bigint,
        CONSTRAINT "PK_DiscordOAuthStates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE TABLE "OutboxEvents" (
        "Id" uuid NOT NULL,
        "EventType" character varying(128) NOT NULL,
        "PayloadJson" text NOT NULL,
        "CreatedAtUnix" bigint NOT NULL,
        "IsPublished" boolean NOT NULL,
        CONSTRAINT "PK_OutboxEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE TABLE "UserAccounts" (
        "Id" uuid NOT NULL,
        "DisplayName" character varying(64) NOT NULL,
        "CreatedAtUnix" bigint NOT NULL,
        "UpdatedAtUnix" bigint NOT NULL,
        CONSTRAINT "PK_UserAccounts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE TABLE "AuthIdentities" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Provider" character varying(32) NOT NULL,
        "ProviderUserId" character varying(128) NOT NULL,
        "DisplayName" character varying(64) NOT NULL,
        "LinkedAtUnix" bigint NOT NULL,
        "UpdatedAtUnix" bigint NOT NULL,
        CONSTRAINT "PK_AuthIdentities" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuthIdentities_UserAccounts_UserId" FOREIGN KEY ("UserId") REFERENCES "UserAccounts" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE TABLE "AuthSessions" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RefreshTokenHash" character varying(128) NOT NULL,
        "DeviceFingerprint" character varying(128) NOT NULL,
        "CreatedAtUnix" bigint NOT NULL,
        "ExpiresAtUnix" bigint NOT NULL,
        "RevokedAtUnix" bigint,
        "LastIdempotencyKey" character varying(128),
        "LastAccessToken" text,
        "LastAccessTokenExpiresAtUnix" bigint,
        CONSTRAINT "PK_AuthSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuthSessions_UserAccounts_UserId" FOREIGN KEY ("UserId") REFERENCES "UserAccounts" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE UNIQUE INDEX "IX_AuthIdentities_Provider_ProviderUserId" ON "AuthIdentities" ("Provider", "ProviderUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE INDEX "IX_AuthIdentities_UserId" ON "AuthIdentities" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE UNIQUE INDEX "IX_AuthSessions_RefreshTokenHash" ON "AuthSessions" ("RefreshTokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE INDEX "IX_AuthSessions_UserId_ExpiresAtUnix" ON "AuthSessions" ("UserId", "ExpiresAtUnix");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE INDEX "IX_DiscordOAuthStates_ExpiresAtUnix" ON "DiscordOAuthStates" ("ExpiresAtUnix");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE UNIQUE INDEX "IX_DiscordOAuthStates_State" ON "DiscordOAuthStates" ("State");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    CREATE INDEX "IX_OutboxEvents_IsPublished_CreatedAtUnix" ON "OutboxEvents" ("IsPublished", "CreatedAtUnix");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260303041514_InitialAuthSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260303041514_InitialAuthSchema', '9.0.0');
    END IF;
END $EF$;
COMMIT;

