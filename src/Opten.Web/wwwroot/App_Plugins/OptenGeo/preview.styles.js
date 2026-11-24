import { css } from "@umbraco-cms/backoffice/external/lit";

export const previewStyles = css`
    :host {
        display: block;
    }

    .toolbar {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: var(--uui-size-space-3);
        margin-bottom: var(--uui-size-space-4);
    }

    .toolbar .spacer {
        flex: 1;
    }

    .checks {
        display: flex;
        flex-wrap: wrap;
        gap: var(--uui-size-space-2);
        margin-top: var(--uui-size-space-4);
    }

    .check {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 3px 10px;
        border-radius: 999px;
        font-size: 12px;
        line-height: 18px;
        border: 1px solid var(--uui-color-border);
        background: var(--uui-color-surface);
    }

    .check::before {
        content: "";
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: var(--uui-color-border-emphasis);
    }

    .check--good::before {
        background: var(--uui-color-positive);
    }

    .check--warning::before {
        background: var(--uui-color-warning-standalone);
    }

    .check--danger::before {
        background: var(--uui-color-danger);
    }

    .hint {
        margin: var(--uui-size-space-3) 0 0;
        color: var(--uui-color-text-alt);
        font-size: 12px;
    }

    .notice {
        padding: var(--uui-size-space-3) var(--uui-size-space-4);
        border-radius: var(--uui-border-radius);
        background: var(--uui-color-warning);
        color: var(--uui-color-warning-contrast);
        margin-bottom: var(--uui-size-space-4);
        font-size: 13px;
    }

    .notice--info {
        background: var(--uui-color-surface-alt);
        color: var(--uui-color-text);
    }
`;
