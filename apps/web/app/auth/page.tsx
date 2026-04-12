import { Suspense } from "react";
import AuthClient from "./AuthClient";

export default function Page() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <AuthClient />
    </Suspense>
  );
}
