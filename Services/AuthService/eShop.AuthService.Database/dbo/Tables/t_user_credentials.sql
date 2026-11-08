CREATE TABLE [dbo].[t_user_credentials] (
    [f_uid]                   UNIQUEIDENTIFIER NOT NULL,
    [f_iid]                   INT              IDENTITY (1, 1) NOT NULL,
    [f_customer_uid]          UNIQUEIDENTIFIER NOT NULL,
    [f_username]              NVARCHAR (150)   NOT NULL,
    [f_password_hash]         NVARCHAR (MAX)   NOT NULL,
    [f_salt]                  NVARCHAR (MAX)   NOT NULL,
    [f_is_locked_out]         BIT              DEFAULT ((0)) NOT NULL,
    [f_failed_login_attempts] INT              DEFAULT ((0)) NOT NULL,
    [f_last_login_date]       DATETIME         NULL,
    [f_create_date]           DATETIME         NOT NULL,
    [f_update_date]           DATETIME         NULL,
    CONSTRAINT [PK_t_user_credentials] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_user_credentials_uid] UNIQUE NONCLUSTERED ([f_uid] ASC),
    CONSTRAINT [UQ_t_user_credentials_username] UNIQUE NONCLUSTERED ([f_username] ASC)
);

