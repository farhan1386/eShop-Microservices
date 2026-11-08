CREATE TABLE [dbo].[t_inventory_stock] (
    [f_uid]         UNIQUEIDENTIFIER NOT NULL,
    [f_iid]         INT              IDENTITY (1, 1) NOT NULL,
    [f_product_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_quantity]    INT              NOT NULL,
    [f_reserved]    INT              NOT NULL,
    [f_row_version] ROWVERSION       NOT NULL,
    [f_create_date] DATETIME         NOT NULL,
    [f_create_by]   UNIQUEIDENTIFIER NULL,
    [f_update_date] DATETIME         NULL,
    [f_update_by]   UNIQUEIDENTIFIER NULL,
    [f_delete_date] DATETIME         NULL,
    [f_delete_by]   UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_t_inventory_stock] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_inventory_stock_product_uid] UNIQUE NONCLUSTERED ([f_product_uid] ASC)
);

