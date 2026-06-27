import os
import glob
import re

files_to_clean = [
    r"d:\CareSphere\Modules\Ward\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Pharmacy\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Patients\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Laboratory\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Nursing\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Notifications\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Billing\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Analytics\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Appointments\Pages\_Imports.razor",
    r"d:\CareSphere\Modules\Clinical\Pages\_Imports.razor",
    r"d:\CareSphere\Components\Pages\Laboratory\_Imports.razor",
    r"d:\CareSphere\Components\Pages\Notifications\_Imports.razor",
    r"d:\CareSphere\Components\Pages\Pharmacy\_Imports.razor",
    r"d:\CareSphere\Components\Pages\Billing\_Imports.razor"
]

for file_path in files_to_clean:
    if os.path.exists(file_path):
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Remove any line starting with @layout
        new_content = re.sub(r'(?m)^@layout\s+.*$', '', content)
        
        if new_content != content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            print(f"Patched {file_path}")

print("All layout overrides removed.")
