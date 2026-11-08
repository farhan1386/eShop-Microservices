CREATE TABLE [dbo].[t_customer] (
    [f_uid]         UNIQUEIDENTIFIER NOT NULL,
    [f_iid]         INT              IDENTITY (1, 1) NOT NULL,
    [f_email]       NVARCHAR (256)   NOT NULL,
    [f_password]    NVARCHAR (MAX)   NOT NULL,
    [f_fname]       NVARCHAR (100)   NOT NULL,
    [f_lname]       NVARCHAR (100)   NOT NULL,
    [f_phone]       NVARCHAR (20)    NULL,
    [f_create_date] DATETIME         NOT NULL,
    [f_create_by]   UNIQUEIDENTIFIER NULL,
    [f_update_date] DATETIME         NULL,
    [f_update_by]   UNIQUEIDENTIFIER NULL,
    [f_delete_date] DATETIME         NULL,
    [f_delete_by]   UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_t_customer] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_customer_email] UNIQUE NONCLUSTERED ([f_email] ASC),
    CONSTRAINT [UQ_t_customer_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

