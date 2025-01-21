function startClock(timeZone) {
    const clockElement = document.getElementById("clock");

    function updateClock() {
        const now = new Date();

        const dateOptions = {
            timeZone: timeZone,
            year: 'numeric',
            month: 'short',
            day: '2-digit',
        };

        const timeOptions = {
            timeZone: timeZone,
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: false,
        };

        const dateFormatter = new Intl.DateTimeFormat('en-US', dateOptions);
        const timeFormatter = new Intl.DateTimeFormat('en-US', timeOptions);

        const formattedDate = dateFormatter.format(now);
        const formattedTime = timeFormatter.format(now);

        clockElement.innerHTML = `${formattedDate}<br>${formattedTime}`;
    }

    updateClock();
    setInterval(updateClock, 1000);
}

document.addEventListener("DOMContentLoaded", function () {
    startClock('@ViewData["time"]');
});

 