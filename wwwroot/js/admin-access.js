$(document).ready(function () {
    $('#submitAdminCode').on('click', function (e) {
        e.preventDefault();
        var accessCode = $('#accessCode').val();

        $.ajax({
            url: '/enter-admin-access-code', // Шлях до дії контролера
            method: 'POST',
            data: { accessCode: accessCode },
            success: function (response) {
                if (response.isValid) {
                    // Якщо код доступу правильний, перенаправляємо на сторінку адміністратора
                    window.location.href = response.redirectUrl;
                } else {
                    $('#errorMessage').text('Неправильний код доступу.');
                }
            },
            error: function () {
                $('#errorMessage').text('Сталася помилка. Спробуйте ще раз.');
            }
        });
    });
});
