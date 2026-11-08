CREATE TABLE [dbo].[t_order_item] (
    [f_uid]         UNIQUEIDENTIFIER NOT NULL,
    [f_iid]         INT              IDENTITY (1, 1) NOT NULL,
    [f_order_uid]   UNIQUEIDENTIFIER NOT NULL,
    [f_product_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_quantity]    INT              NOT NULL,
    [f_unit_price]  DECIMAL (18, 2)  NOT NULL,
    [f_create_date] DATETIME         NOT NULL,
    [f_create_by]   UNIQUEIDENTIFIER NULL,
    [f_update_date] DATETIME         NULL,
    [f_update_by]   UNIQUEIDENTIFIER NULL,
    [f_delete_date] DATETIME         NULL,
    [f_delete_by]   UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_t_order_item] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [FK_t_order_item_t_order] FOREIGN KEY ([f_order_uid]) REFERENCES [dbo].[t_order] ([f_uid]),
    CONSTRAINT [UQ_t_order_item_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

