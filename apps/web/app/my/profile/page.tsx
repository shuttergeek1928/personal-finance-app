"use client";

import { useEffect, useState } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import {
  userService,
  UserTransferObject,
  UpdateUserProfileRequest,
} from "@/services/user";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogClose,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  User as UserIcon,
  Calendar,
  Globe,
  DollarSign,
  Activity,
  Edit,
  ShieldCheck,
  Mail,
  Phone,
  Clock,
  AlertTriangle,
  Trash2,
} from "lucide-react";

export default function MyProfilePage() {
  const { user: authUser, initialize, logout: authLogout } = useAuthStore();
  const router = useRouter();
  const [user, setUser] = useState<UserTransferObject | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [editData, setEditData] = useState<UpdateUserProfileRequest>({
    firstName: "",
    lastName: "",
    phoneNumber: "",
    dateOfBirth: "",
    currency: "INR",
    timeZone: "UTC",
    language: "en-US",
  });

  const fetchData = async () => {
    if (!authUser) return;
    setLoading(true);
    try {
      const res = await userService.getUserById(authUser.id);
      if (res.success && res.data) {
        setUser(res.data);
        const p = res.data.profile;
        setEditData({
          firstName: res.data.firstName || "",
          lastName: res.data.lastName || "",
          phoneNumber: res.data.phoneNumber || "",
          dateOfBirth: p?.dateOfBirth ? p.dateOfBirth.split("T")[0] : "",
          currency: p?.currency || "INR",
          timeZone: p?.timeZone || "UTC",
          language: p?.language || "en-US",
        });
      } else {
        setError(res.message || "Failed to fetch profile details.");
      }
    } catch (err: any) {
      setError(err.message || "An error occurred while fetching your profile.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [authUser]);

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    setIsSubmitting(true);
    try {
      const payload: UpdateUserProfileRequest = {
        firstName: editData.firstName,
        lastName: editData.lastName,
        phoneNumber: editData.phoneNumber,
        dateOfBirth: editData.dateOfBirth
          ? new Date(editData.dateOfBirth).toISOString()
          : null,
        currency: editData.currency,
        timeZone: editData.timeZone,
        language: editData.language,
      };

      const res = await userService.updateProfile(user.id, payload);
      if (res.success) {
        setIsEditOpen(false);
        fetchData();
        // Update local store if needed (store usually just has basics, but let's re-initialize for safe measure)
        initialize();
      } else {
        alert(res.message || "Failed to update profile");
      }
    } catch (err: any) {
      alert(err.message || "Failed to update profile");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (!user) return;
    
    setIsDeleting(true);
    try {
      const res = await userService.deleteMe();
      if (res.success) {
        authLogout();
        router.push("/");
      } else {
        alert(res.message || "Failed to delete account");
      }
    } catch (err: any) {
      alert(err.message || "Failed to delete account");
    } finally {
      setIsDeleting(false);
      setIsDeleteOpen(false);
    }
  };

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[50vh]">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">
            Loading your profile...
          </p>
        </div>
      </div>
    );
  }

  if (error || !user) {
    return (
      <div className="p-8 max-w-4xl mx-auto">
        <div className="bg-destructive/10 text-destructive p-4 rounded-md flex items-center gap-2">
          <Activity className="w-5 h-5" />
          {error || "Profile not found"}
        </div>
      </div>
    );
  }

  const p = user.profile;

  return (
    <div className="p-6 md:p-8 max-w-[1600px] mx-auto space-y-8 animate-in fade-in duration-500">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">My Profile</h1>
          <p className="text-muted-foreground mt-1">
            Manage your personal information and preferences.
          </p>
        </div>

        <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
          <DialogTrigger asChild>
            <Button className="bg-indigo-600 hover:bg-indigo-700 text-white gap-2 shadow-md">
              <Edit className="w-4 h-4" /> Edit Profile
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Update Profile</DialogTitle>
              <DialogDescription>
                Modify your account details and financial preferences.
              </DialogDescription>
            </DialogHeader>
            <form onSubmit={handleUpdate} className="space-y-4 pt-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">First Name</Label>
                  <Input
                    id="firstName"
                    value={editData.firstName || ""}
                    onChange={(e) =>
                      setEditData({ ...editData, firstName: e.target.value })
                    }
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">Last Name</Label>
                  <Input
                    id="lastName"
                    value={editData.lastName || ""}
                    onChange={(e) =>
                      setEditData({ ...editData, lastName: e.target.value })
                    }
                    required
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="phone">Phone Number</Label>
                <Input
                  id="phone"
                  value={editData.phoneNumber || ""}
                  onChange={(e) =>
                    setEditData({ ...editData, phoneNumber: e.target.value })
                  }
                  placeholder="+91 XXXX XXX XXX"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="dob">Date of Birth</Label>
                <Input
                  id="dob"
                  type="date"
                  value={editData.dateOfBirth || ""}
                  onChange={(e) =>
                    setEditData({ ...editData, dateOfBirth: e.target.value })
                  }
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="currency">Currency</Label>
                  <Input
                    id="currency"
                    value={editData.currency || ""}
                    onChange={(e) =>
                      setEditData({ ...editData, currency: e.target.value })
                    }
                    placeholder="INR"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="language">Language</Label>
                  <Input
                    id="language"
                    value={editData.language || ""}
                    onChange={(e) =>
                      setEditData({ ...editData, language: e.target.value })
                    }
                    placeholder="en-US"
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="timezone">Time Zone</Label>
                <Input
                  id="timezone"
                  value={editData.timeZone || ""}
                  onChange={(e) =>
                    setEditData({ ...editData, timeZone: e.target.value })
                  }
                  placeholder="UTC"
                />
              </div>
              <DialogFooter className="pt-4">
                <DialogClose asChild>
                  <Button variant="outline" type="button">
                    Cancel
                  </Button>
                </DialogClose>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Saving..." : "Save Changes"}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Account Info Card */}
        <Card className="shadow-sm border-zinc-200 dark:border-zinc-800">
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <UserIcon className="w-5 h-5 text-indigo-600" /> Account
              Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-5">
            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <UserIcon className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Full Name
                  </span>
                  <span className="font-medium">{user.fullName || "N/A"}</span>
                </div>
              </div>
              {user.isActive && (
                <span title="Account Active">
                  <ShieldCheck className="w-4 h-4 text-emerald-500" />
                </span>
              )}
            </div>

            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <Mail className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Email Address
                  </span>
                  <span className="font-medium truncate max-w-[180px]">
                    {user.email || "N/A"}
                  </span>
                </div>
              </div>
              {user.isEmailConfirmed && (
                <span title="Email Confirmed">
                  <ShieldCheck className="w-4 h-4 text-emerald-500" />
                </span>
              )}
            </div>

            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <Phone className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Phone Number
                  </span>
                  <span className="font-medium">
                    {user.phoneNumber || "N/A"}
                  </span>
                </div>
              </div>
            </div>

            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <Clock className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Member Since
                  </span>
                  <span className="font-medium">
                    {new Date(user.createdAt).toLocaleDateString()}
                  </span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Profile Settings Card */}
        <Card className="shadow-sm border-zinc-200 dark:border-zinc-800">
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <Activity className="w-5 h-5 text-indigo-600" /> Profile
              Preferences
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-5">
            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <Calendar className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Date of Birth
                  </span>
                  <span className="font-medium">
                    {p?.dateOfBirth
                      ? new Date(p.dateOfBirth).toLocaleDateString()
                      : "N/A"}
                    {p?.age ? (
                      <span className="text-muted-foreground ml-1">
                        ({p.age} years)
                      </span>
                    ) : (
                      ""
                    )}
                  </span>
                </div>
              </div>
            </div>

            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <DollarSign className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Default Currency
                  </span>
                  <span className="font-medium">{p?.currency || "INR"}</span>
                </div>
              </div>
            </div>

            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <Globe className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Language
                  </span>
                  <span className="font-medium">{p?.language || "en-US"}</span>
                </div>
              </div>
            </div>

            <div className="flex items-center justify-between pb-2 border-b border-zinc-100 dark:border-zinc-900 last:border-0 last:pb-0">
              <div className="flex items-center gap-3">
                <div className="bg-zinc-100 dark:bg-zinc-900 p-2 rounded-lg">
                  <Globe className="w-4 h-4 text-zinc-500" />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">
                    Time Zone
                  </span>
                  <span className="font-medium">{p?.timeZone || "UTC"}</span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Danger Zone */}
      <Card className="shadow-sm border-red-200 dark:border-red-900 overflow-hidden">
        <div className="bg-red-50 dark:bg-red-950/20 px-6 py-3 border-b border-red-100 dark:border-red-900">
          <CardTitle className="text-lg flex items-center gap-2 text-red-700 dark:text-red-400">
            <AlertTriangle className="w-5 h-5" /> Danger Zone
          </CardTitle>
        </div>
        <CardContent className="p-6 space-y-4">
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div className="space-y-1">
              <h4 className="font-semibold text-zinc-900 dark:text-zinc-100">
                Delete Account
              </h4>
              <p className="text-sm text-muted-foreground mr-4">
                Permanently delete your account and all associated data including
                transactions, accounts, and obligations. This action cannot be
                undone.
              </p>
            </div>

            <Dialog open={isDeleteOpen} onOpenChange={setIsDeleteOpen}>
              <DialogTrigger asChild>
                <Button variant="destructive" className="gap-2 shrink-0">
                  <Trash2 className="w-4 h-4" /> Delete My Account
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-md">
                <DialogHeader>
                  <DialogTitle className="flex items-center gap-2 text-red-600">
                    <AlertTriangle className="w-5 h-5" /> Are you absolutely sure?
                  </DialogTitle>
                  <DialogDescription className="pt-2">
                    This will permanently delete your account (
                    <strong>{user.email}</strong>) and remove all your data from
                    our servers. This includes all your transactions, bank accounts,
                    and financial history.
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter className="flex flex-col sm:flex-row gap-3 pt-4">
                  <DialogClose asChild>
                    <Button variant="outline" className="sm:flex-1">
                      Cancel
                    </Button>
                  </DialogClose>
                  <Button
                    variant="destructive"
                    className="sm:flex-1"
                    onClick={handleDeleteAccount}
                    disabled={isDeleting}
                  >
                    {isDeleting ? "Deleting..." : "Yes, Delete Everything"}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
