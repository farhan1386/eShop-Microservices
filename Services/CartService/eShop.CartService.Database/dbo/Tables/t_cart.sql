CREATE TABLE [dbo].[t_cart] (
    [f_uid]          UNIQUEIDENTIFIER NOT NULL,
    [f_iid]          INT              IDENTITY (1, 1) NOT NULL,
    [f_customer_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_create_date]  DATETIME         NOT NULL,
    [f_update_date]  DATETIME         NULL,
    CONSTRAINT [PK_t_cart] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_cart_customer] UNIQUE NONCLUSTERED ([f_customer_uid] ASC),
    CONSTRAINT [UQ_t_cart_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

