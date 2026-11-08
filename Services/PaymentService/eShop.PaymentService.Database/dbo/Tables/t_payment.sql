CREATE TABLE [dbo].[t_payment] (
    [f_uid]          UNIQUEIDENTIFIER NOT NULL,
    [f_iid]          INT              IDENTITY (1, 1) NOT NULL,
    [f_order_uid]    UNIQUEIDENTIFIER NOT NULL,
    [f_amount]       DECIMAL (18, 2)  NOT NULL,
    [f_method]       NVARCHAR (50)    NOT NULL,
    [f_status]       NVARCHAR (50)    NOT NULL,
    [f_gateway_txid] NVARCHAR (255)   NULL,
    [f_create_date]  DATETIME         NOT NULL,
    [f_create_by]    UNIQUEIDENTIFIER NULL,
    [f_update_date]  DATETIME         NULL,
    [f_update_by]    UNIQUEIDENTIFIER NULL,
    [f_delete_date]  DATETIME         NULL,
    [f_delete_by]    UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_t_payment] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_payment_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

