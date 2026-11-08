CREATE TABLE [dbo].[t_product] (
    [f_uid]          UNIQUEIDENTIFIER NOT NULL,
    [f_iid]          INT              IDENTITY (1, 1) NOT NULL,
    [f_category_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_name]         NVARCHAR (255)   NOT NULL,
    [f_sku]          NVARCHAR (100)   NOT NULL,
    [f_price]        DECIMAL (18, 2)  NOT NULL,
    [f_attributes]   NVARCHAR (MAX)   NULL,
    [f_create_date]  DATETIME         NOT NULL,
    [f_create_by]    UNIQUEIDENTIFIER NULL,
    [f_update_date]  DATETIME         NULL,
    [f_update_by]    UNIQUEIDENTIFIER NULL,
    [f_delete_date]  DATETIME         NULL,
    [f_delete_by]    UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_t_product] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [CK_t_product_attributes_json] CHECK (isjson([f_attributes])=(1)),
    CONSTRAINT [FK_t_product_t_product_category] FOREIGN KEY ([f_category_uid]) REFERENCES [dbo].[t_product_category] ([f_uid]),
    CONSTRAINT [UQ_t_product_sku] UNIQUE NONCLUSTERED ([f_sku] ASC),
    CONSTRAINT [UQ_t_product_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

