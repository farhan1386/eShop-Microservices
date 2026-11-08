CREATE TABLE [dbo].[t_order_history] (
    [f_uid]         UNIQUEIDENTIFIER NOT NULL,
    [f_iid]         INT              IDENTITY (1, 1) NOT NULL,
    [f_order_uid]   UNIQUEIDENTIFIER NOT NULL,
    [f_old_status]  NVARCHAR (50)    NOT NULL,
    [f_new_status]  NVARCHAR (50)    NOT NULL,
    [f_comment]     NVARCHAR (500)   NULL,
    [f_create_date] DATETIME         NOT NULL,
    CONSTRAINT [PK_t_order_history] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [UQ_t_order_history_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

