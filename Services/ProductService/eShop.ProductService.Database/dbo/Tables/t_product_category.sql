CREATE TABLE [dbo].[t_product_category] (
    [f_uid]         UNIQUEIDENTIFIER NOT NULL,
    [f_iid]         INT              IDENTITY (1, 1) NOT NULL,
    [f_name]        NVARCHAR (100)   NOT NULL,
    [f_create_date] DATETIME         NOT NULL,
    [f_create_by]   UNIQUEIDENTIFIER NULL,
    [f_update_date] DATETIME         NULL,
    [f_update_by]   UNIQUEIDENTIFIER NULL,
    [f_delete_date] DATETIME         NULL,
    [f_delete_by]   UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_t_product_category] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_product_category_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

