CREATE TABLE [dbo].[t_shipping_history] (
    [f_uid]                   UNIQUEIDENTIFIER NOT NULL,
    [f_iid]                   INT              IDENTITY (1, 1) NOT NULL,
    [f_shipping_manifest_uid] UNIQUEIDENTIFIER NOT NULL,
    [f_old_status]            NVARCHAR (50)    NOT NULL,
    [f_new_status]            NVARCHAR (50)    NOT NULL,
    [f_status_location]       NVARCHAR (255)   NULL,
    [f_comment]               NVARCHAR (500)   NULL,
    [f_create_date]           DATETIME         NOT NULL,
    CONSTRAINT [PK_t_shipping_history] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_shipping_history_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

