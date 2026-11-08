CREATE TABLE [dbo].[t_customer_address] (
    [f_uid]          UNIQUEIDENTIFIER NOT NULL,
    [f_iid]          INT              IDENTITY (1, 1) NOT NULL,
    [f_customer_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_street]       NVARCHAR (255)   NOT NULL,
    [f_city]         NVARCHAR (100)   NOT NULL,
    [f_state]        NVARCHAR (100)   NOT NULL,
    [f_postal_code]  NVARCHAR (20)    NOT NULL,
    [f_create_date]  DATETIME         NOT NULL,
    [f_create_by]    UNIQUEIDENTIFIER NULL,
    [f_update_date]  DATETIME         NULL,
    [f_update_by]    UNIQUEIDENTIFIER NULL,
    [f_delete_date]  DATETIME         NULL,
    [f_delete_by]    UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_t_customer_address] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [FK_t_customer_address_t_customer] FOREIGN KEY ([f_customer_uid]) REFERENCES [dbo].[t_customer] ([f_uid]),
    CONSTRAINT [UQ_t_customer_address_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

