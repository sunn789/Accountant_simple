
#!/usr/bin/env bash

set -u

OUTPUT="ui-design-context.txt"

FILES=(
    # UI - configuration
    "ui/package.json"
    "ui/src/App.tsx"
    "ui/src/App.css"
    "ui/src/index.css"
    "ui/src/main.tsx"

    # UI - API and types
    "ui/src/services/api.ts"
    "ui/src/types/invoice.ts"

    # UI - components
    "ui/src/components/Navbar.tsx"
    "ui/src/components/Invoice/InvoiceForm.tsx"
    "ui/src/components/Invoice/InvoiceTemplate.tsx"

    # UI - pages
    "ui/src/pages/Home.tsx"
    "ui/src/pages/Dashboard.tsx"
    "ui/src/pages/InvoiceList.tsx"
    "ui/src/pages/CreateInvoice.tsx"
    "ui/src/pages/InvoiceDetail.tsx"

    # API - controllers and configuration
    "Api/Program.cs"
    "Api/Controllers/DashboardController.cs"
    "Api/Controllers/InvoiceController.cs"
    "Api/Controllers/PublicInvoicesController.cs"

    # API - DTOs
    "Api/DTOs/DashboardStatsDto.cs"
    "Api/DTOs/PagedResultDto.cs"

    # API - models
    "Api/Models/Invoice.cs"
    "Api/Models/InvoiceItem.cs"
    "Api/Models/Party.cs"
    "Api/Models/CompanyProfile.cs"
    "Api/Models/BankAccount.cs"
    "Api/Models/InvoiceDocument.cs"
    "Api/Models/InvoiceSmsLog.cs"
)

: > "$OUTPUT"

{
    echo "=================================================="
    echo "UI DESIGN CONTEXT"
    echo "Project: Invoice Management System"
    echo "=================================================="
    echo

    for FILE in "${FILES[@]}"; do
        echo
        echo "=================================================="
        echo "FILE: $FILE"
        echo "=================================================="

        if [[ -f "$FILE" ]]; then
            cat "$FILE"
        else
            echo "[FILE NOT FOUND]"
        fi

        echo
    done

} >> "$OUTPUT"

echo "Context file created: $OUTPUT"
echo "Included files: ${#FILES[@]}"
echo
echo "You can now copy the content of:"
echo "$OUTPUT"
