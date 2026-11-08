CREATE TABLE [dbo].[t_role] (
    [f_uid]         UNIQUEIDENTIFIER NOT NULL,
    [f_iid]         INT              IDENTITY (1, 1) NOT NULL,
    [f_name]        NVARCHAR (100)   NOT NULL,
    [f_description] NVARCHAR (255)   NULL,
    [f_create_date] DATETIME         NOT NULL,
    CONSTRAINT [PK_t_role] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_role_name] UNIQUE NONCLUSTERED ([f_name] ASC),
    CONSTRAINT [UQ_t_role_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

