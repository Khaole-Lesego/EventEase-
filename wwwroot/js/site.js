// EventEase carousel interactions inspired by cinematic full-screen sliders.
(function () {
    const carousels = document.querySelectorAll('[data-carousel]');

    carousels.forEach((carouselDom) => {
        const sliderDom = carouselDom.querySelector('[data-carousel-list]');
        const thumbnailBorderDom = carouselDom.querySelector('[data-carousel-thumbnail]');
        const nextDom = carouselDom.querySelector('[data-carousel-next]');
        const prevDom = carouselDom.querySelector('[data-carousel-prev]');
        const timeDom = carouselDom.querySelector('[data-carousel-time]');

        if (!sliderDom || !thumbnailBorderDom || !nextDom || !prevDom || !timeDom) {
            return;
        }

        const thumbnailItemsDom = thumbnailBorderDom.querySelectorAll('.item');
        if (thumbnailItemsDom.length > 0) {
            thumbnailBorderDom.appendChild(thumbnailItemsDom[0]);
        }

        const timeRunning = 3000;
        const timeAutoNext = 7000;
        let runTimeOut;
        let runNextAuto;

        const showSlider = (type) => {
            const sliderItemsDom = sliderDom.querySelectorAll('.item');
            const thumbs = thumbnailBorderDom.querySelectorAll('.item');

            if (sliderItemsDom.length <= 1 || thumbs.length <= 1) {
                return;
            }

            if (type === 'next') {
                sliderDom.appendChild(sliderItemsDom[0]);
                thumbnailBorderDom.appendChild(thumbs[0]);
                carouselDom.classList.add('next');
            } else {
                sliderDom.prepend(sliderItemsDom[sliderItemsDom.length - 1]);
                thumbnailBorderDom.prepend(thumbs[thumbs.length - 1]);
                carouselDom.classList.add('prev');
            }

            clearTimeout(runTimeOut);
            runTimeOut = setTimeout(() => {
                carouselDom.classList.remove('next');
                carouselDom.classList.remove('prev');
            }, timeRunning);

            clearTimeout(runNextAuto);
            runNextAuto = setTimeout(() => {
                nextDom.click();
            }, timeAutoNext);
        };

        nextDom.addEventListener('click', () => showSlider('next'));
        prevDom.addEventListener('click', () => showSlider('prev'));

        runNextAuto = setTimeout(() => {
            nextDom.click();
        }, timeAutoNext);
    });
})();
