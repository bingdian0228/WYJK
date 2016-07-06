
$(function () {

    $("#province").change(function () {
        
        var code = $(this).val();
        $("#provinceText").val($(this).find("option:selected").text());
        $.ajax({
            url: '/Calculator/CitysByProvince/' + code,
            dataType: 'Json',
            success: function (data) {
                $("#city").empty();
                $("#city").append("<option>请选择城市</option>");
                $.each(data, function (i, item) {
                    $("#city").append("<option value='" + item.RegionName + "'>" + item.RegionName + "</option>");
                    
                });
                
            }
        });

    });


    




});
