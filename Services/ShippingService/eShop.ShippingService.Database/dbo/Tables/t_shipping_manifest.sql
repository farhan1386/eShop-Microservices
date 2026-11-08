CREATE TABLE [dbo].[t_shipping_manifest] (
    [f_uid]                UNIQUEIDENTIFIER NOT NULL,
    [f_iid]                INT              IDENTITY (1, 1) NOT NULL,
    [f_order_uid]          UNIQUEIDENTIFIER NOT NULL,
    [f_carrier_name]       NVARCHAR (100)   NOT NULL,
    [f_tracking_number]    NVARCHAR (100)   NULL,
    [f_shipping_status]    NVARCHAR (50)    NOT NULL,
    [f_estimated_delivery] DATETIME         NULL,
    [f_create_date]        DATETIME         NOT NULL,
    [f_update_date]        DATETIME         NULL,
    CONSTRAINT [PK_t_shipping_manifest] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_shipping_manifest_order] UNIQUE NONCLUSTERED ([f_order_uid] ASC),
    CONSTRAINT [UQ_t_shipping_manifest_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

