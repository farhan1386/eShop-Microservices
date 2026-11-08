CREATE TABLE [dbo].[t_outbox_event] (
    [f_uid]            UNIQUEIDENTIFIER NOT NULL,
    [f_iid]            INT              IDENTITY (1, 1) NOT NULL,
    [f_event_type]     NVARCHAR (255)   NOT NULL,
    [f_payload]        NVARCHAR (MAX)   NOT NULL,
    [f_status]         NVARCHAR (50)    DEFAULT ('Pending') NOT NULL,
    [f_create_date]    DATETIME         NOT NULL,
    [f_processed_date] DATETIME         NULL,
    [f_error_log]      NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_t_outbox_event] PRIMARY KEY CLUSTERED ([f_iid] ASC),
    CONSTRAINT [CK_t_outbox_event_payload_json] CHECK (isjson([f_payload])=(1)),
    CONSTRAINT [UQ_t_outbox_event_uid] UNIQUE NONCLUSTERED ([f_uid] ASC)
);

