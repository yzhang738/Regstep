var newCompanyDatabase = null;

$(document).ready(function () {
    $('#companyName').on('keyup', function () {
        var val = $(this).val();
        $('#newCompanyName').html(val);
        val = val.replace(/[^a-zA-Z0-9]/g, "");
        $('#companyDatabase').val(val);
    });

    $('#createCompanyForm').on('validatedsubmit', function (ui, e) {
        $.ajax({
            url: '../Admin/CreateCompany',
            type: "post",
            data: JSON.stringify($(this).serializeJSON()),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    $('#creationStatus').html('Company Created. Building Database.');
                    newCompanyDatabase = jsonResult.Database;
                    CreateDatabase();
                } else {
                    alert("Company Failed To Create");
                }
            }
        });
    });
});

function CreateDatabase() {
    $.ajax({
        url: '../Admin/BuildNewDatabase',
        type: "post",
        data: JSON.stringify({ companyDB: newCompanyDatabase }),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                $('#creationStatus').html('Database Created. Building Tables.');
                CreateTables();
            } else {
                alert("Datebase Failed to Build.");
            }
        }
    });
}

function CreateTables() {
    $.ajax({
        url: '../Admin/BuildNewTables',
        type: "post",
        data: JSON.stringify({ companyDB: newCompanyDatabase }),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                $('#creationStatus').html('Tables Created. Populating Tables.');
                PopulateTables();
            } else {
                alert("Tables Failed to Build.");
            }
        }
    });
}

function PopulateTables() {
    $.ajax({
        url: '../Admin/PopulateNewTables',
        type: "post",
        data: JSON.stringify({ companyDB: newCompanyDatabase }),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                $('#creationStatus').html('Populated Tables. FINISHED.');
            } else {
                alert("Tables Failed to Populate.");
            }
        }
    });
}