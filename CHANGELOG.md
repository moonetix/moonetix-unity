# Changelog

All notable changes to this project are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/).

## [2.0.1] - 2026-11-11

### Changed

- Android: uses `com.moonetix:offerwall:2.0.1`. iOS: bundles `MoonetixOfferwall.xcframework` 2.0.1.
- Only pages on `wall.moonetix.com` stay inside the offerwall screen. Every other host opens outside the app.
- The top bar logo loads only from `moonetix.com` and `wall.moonetix.com`.

## [2.0.0] - 2026-11-06

### Changed

- First release under the Moonetix name. The previous package stays published and keeps working.
- Package `com.moonetix.offerwall`, namespace `MoonetixOfferwall`, class `Moonetix`, types `MoonetixOptions` and
  `MoonetixError`.
- Android: uses `com.moonetix:offerwall:2.0.0` from https://moonetix.com/maven, added to the Gradle project
  automatically.
- iOS: bundles `MoonetixOfferwall.xcframework` 2.0.0, linked and embedded automatically. iOS 13 or newer.
- The wall opens on `wall.moonetix.com`.
