CREATE TABLE [dbo].[t_user_role] (
    [f_uid]                  UNIQUEIDENTIFIER NOT NULL,
    [f_iid]                  INT              IDENTITY (1, 1) NOT NULL,
    [f_user_credentials_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_role_uid]             UNIQUEIDENTIFIER NOT NULL,
    [f_create_date]          DATETIME         NOT NULL,
    CONSTRAINT [PK_t_user_role] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_user_role_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

