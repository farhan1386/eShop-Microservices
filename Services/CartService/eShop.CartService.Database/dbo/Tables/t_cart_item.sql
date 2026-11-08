CREATE TABLE [dbo].[t_cart_item] (
    [f_uid]            UNIQUEIDENTIFIER NOT NULL,
    [f_iid]            INT              IDENTITY (1, 1) NOT NULL,
    [f_cart_uid]       UNIQUEIDENTIFIER NOT NULL,
    [f_product_uid]    UNIQUEIDENTIFIER NOT NULL,
    [f_quantity]       INT              NOT NULL,
    [f_price_snapshot] DECIMAL (18, 2)  NOT NULL,
    [f_create_date]    DATETIME         NOT NULL,
    [f_update_date]    DATETIME         NULL,
    CONSTRAINT [PK_t_cart_item] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [CK_t_cart_item_quantity] CHECK ([f_quantity]>(0)),
    CONSTRAINT [UQ_t_cart_item_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

