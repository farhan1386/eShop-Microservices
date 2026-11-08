CREATE TABLE [dbo].[t_refresh_token] (
    [f_uid]                  UNIQUEIDENTIFIER NOT NULL,
    [f_iid]                  INT              IDENTITY (1, 1) NOT NULL,
    [f_user_credentials_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_token]                NVARCHAR (500)   NOT NULL,
    [f_expires_date]         DATETIME         NOT NULL,
    [f_is_revoked]           BIT              DEFAULT ((0)) NOT NULL,
    [f_create_date]          DATETIME         NOT NULL,
    CONSTRAINT [PK_t_refresh_token] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_refresh_token_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

